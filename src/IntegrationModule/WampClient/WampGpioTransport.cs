using ConnectPro.Enums;
using ConnectPro.Models;
using SharedComponents.Models.GPIO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wamp.Client;
using static Wamp.Client.WampClient;

public sealed class WampGpioTransport : IGpioTransport
{
    private readonly WampClient _client;

    /// <summary>
    /// Callback per device dirno.
    /// The SDK uses dirno as the routing key because trace payloads do not include dirno.
    /// </summary>
    private readonly ConcurrentDictionary<string, Action<GpioPoint>> _callbacks =
        new ConcurrentDictionary<string, Action<GpioPoint>>();

    public WampGpioTransport(WampClient client)
    {
        _client = client ?? throw new ArgumentNullException("client");

        // IMPORTANT:
        // Use *Ex events because wamp_device_gpio_element does NOT contain dirno.
        // The WampClient tracer captures dirno at subscription time and emits it via WampGpioEventArgs.
        _client.OnWampDeviceGPIStatusEventEx += OnGpiEventEx;
        _client.OnWampDeviceGPOStatusEventEx += OnGpoEventEx;
    }

    /// <summary>
    /// Returns a "best effort" snapshot of known GPIO points by querying both GPIs and GPOs.
    /// The WampClient wrappers are synchronous; we keep an async surface for SDK stability.
    /// </summary>
    public Task<IReadOnlyList<GpioPoint>> GetSnapshotAsync(string dirno, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(dirno))
            throw new ArgumentException("dirno must be provided.", "dirno");

        // If caller cancelled early, stop immediately.
        if (ct.IsCancellationRequested)
            return Task.FromCanceled<IReadOnlyList<GpioPoint>>(ct);

        var list = new List<GpioPoint>();

        // Current SDK wrappers are sync and return empty list on null responses.
        var gpis = _client.requestDevicesGPIs(dirno, null);
        var gpos = _client.requestDevicesGPOs(dirno, null);

        MapSnapshot(list, gpis, GpioDirection.Gpi);
        MapSnapshot(list, gpos, GpioDirection.Gpo);

        return Task.FromResult((IReadOnlyList<GpioPoint>)list);
    }

    /// <summary>
    /// Sets one GPO output point (activate/deactivate) using the ZCP WAMP endpoint.
    /// </summary>
    public Task SetGpoAsync(string dirno, string gpoId, bool active, int? timeSeconds, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(dirno))
            throw new ArgumentException("dirno must be provided.", "dirno");

        if (ct.IsCancellationRequested)
            return Task.FromCanceled(ct);

        // Use gpoId directly (e.g., "relay1", "gpio2")
        string id = gpoId;

        // ZCP expects operation strings like "set" / "clear".
        string operation = active ? "set" : "clear";

        // Optional time parameter; keep 0 as "no time limit" unless API says otherwise.
        int time = timeSeconds ?? 0;

        _client.PostDeviceGPO(dirno, id, operation, time);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Ensures trace subscriptions are active for a specific device and registers the callback
    /// used to route incoming GPI/GPO changes to the runtime GPIO model.
    /// </summary>
    public void EnsureSubscribed(string dirno, Action<GpioPoint> onPoint)
    {
        if (string.IsNullOrEmpty(dirno))
            throw new ArgumentException("dirno must be provided.", "dirno");
        if (onPoint == null)
            throw new ArgumentNullException("onPoint");

        // Register/replace callback first so events arriving immediately can be routed.
        _callbacks[dirno] = onPoint;

        // Global subscriptions for GPI/GPO events (idempotent - WampClient guards duplicates).
        _client.TraceDeviceGPIStatusEvent();
        _client.TraceDeviceGPOStatusEvent();
    }

    /// <summary>
    /// Removes the callback for this device. The global WAMP subscription remains active
    /// for other devices that may still be subscribed.
    /// </summary>
    public void DisposeFor(string dirno)
    {
        if (string.IsNullOrEmpty(dirno))
            return;

        Action<GpioPoint> _;
        _callbacks.TryRemove(dirno, out _);
    }

    // ---------------- EventEx handlers (include dirno) ----------------

    private void OnGpiEventEx(object sender, WampGpioEventArgs e)
    {
        if (e == null)
            return;

        Dispatch(e.Dirno, e.Element, GpioDirection.Gpi);
    }

    private void OnGpoEventEx(object sender, WampGpioEventArgs e)
    {
        if (e == null)
            return;

        Dispatch(e.Dirno, e.Element, GpioDirection.Gpo);
    }

    private void Dispatch(string dirno, wamp_device_gpio_element element, GpioDirection direction)
    {
        if (string.IsNullOrEmpty(dirno) || element == null)
            return;

        Action<GpioPoint> cb;
        if (!_callbacks.TryGetValue(dirno, out cb))
            return;

        var point = new GpioPoint(
            element.id,
            direction,
            ParseState(element),
            DateTimeOffset.UtcNow,
            element.ToString());

        cb(point);
    }

    // ---------------- Parsing helpers ----------------

    private static int ParseIndex(string id)
    {
        if (string.IsNullOrEmpty(id))
            return -1;

        // Examples: "relay2" -> 2, "gpio5" -> 5
        for (int i = 0; i < id.Length; i++)
        {
            if (char.IsDigit(id[i]))
                return int.Parse(id.Substring(i));
        }

        return -1;
    }

    private static GpioState ParseState(wamp_device_gpio_element e)
    {
        if (e == null) return GpioState.Inactive;

        // Primary: use "state" field when present ("low"/"high")
        if (!string.IsNullOrEmpty(e.state))
        {
            if (e.state.Equals("high", StringComparison.OrdinalIgnoreCase))
                return GpioState.Active;

            if (e.state.Equals("low", StringComparison.OrdinalIgnoreCase))
                return GpioState.Inactive;

            // fallback: some systems use "1"/"0"
            if (e.state == "1") return GpioState.Active;
            if (e.state == "0") return GpioState.Inactive;
        }

        // Fallback: GPO events use "operation" ("set"/"clear") instead of "state"
        if (!string.IsNullOrEmpty(e.operation))
        {
            if (e.operation.Equals("set", StringComparison.OrdinalIgnoreCase))
                return GpioState.Active;

            if (e.operation.Equals("clear", StringComparison.OrdinalIgnoreCase))
                return GpioState.Inactive;
        }

        return GpioState.Inactive;
    }


    private static void MapSnapshot(List<GpioPoint> list, IEnumerable<wamp_device_gpio_element> elements, GpioDirection direction)
    {
        if (elements == null)
            return;

        foreach (var e in elements)
        {
            if (e == null)
                continue;

            list.Add(new GpioPoint(
                e.id,
                direction,
                ParseState(e),
                DateTimeOffset.UtcNow,
                e.ToString()));
        }
    }
}
