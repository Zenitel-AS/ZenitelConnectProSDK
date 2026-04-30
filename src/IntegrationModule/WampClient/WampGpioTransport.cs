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

/// <summary>
/// Provides a WAMP-backed GPIO transport for snapshots, output control, and realtime GPIO event routing.
/// </summary>
public sealed class WampGpioTransport : IGpioTransport, IDisposable
{
    private readonly WampClient _client;
    private readonly ConcurrentDictionary<string, Action<GpioPoint>> _callbacks =
        new ConcurrentDictionary<string, Action<GpioPoint>>();

    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="WampGpioTransport"/> class.
    /// </summary>
    /// <param name="client">The WAMP client used for GPIO requests and realtime subscriptions.</param>
    public WampGpioTransport(WampClient client)
    {
        _client = client ?? throw new ArgumentNullException("client");

        _client.OnWampDeviceGPIStatusEventEx += OnGpiEventEx;
        _client.OnWampDeviceGPOStatusEventEx += OnGpoEventEx;
    }

    /// <summary>
    /// Retrieves a snapshot of GPIO input and output points for the specified device.
    /// </summary>
    /// <param name="dirno">The directory number that identifies the target device.</param>
    /// <param name="ct">The cancellation token used to cancel the operation.</param>
    /// <returns>A task that returns the current GPIO snapshot for the device.</returns>
    public Task<IReadOnlyList<GpioPoint>> GetSnapshotAsync(string dirno, CancellationToken ct)
    {
        ThrowIfDisposed();

        if (string.IsNullOrEmpty(dirno))
            throw new ArgumentException("dirno must be provided.", "dirno");

        if (ct.IsCancellationRequested)
            return Task.FromCanceled<IReadOnlyList<GpioPoint>>(ct);

        var list = new List<GpioPoint>();

        var gpis = _client.requestDevicesGPIs(dirno, null);
        var gpos = _client.requestDevicesGPOs(dirno, null);

        MapSnapshot(list, gpis, GpioDirection.Gpi);
        MapSnapshot(list, gpos, GpioDirection.Gpo);

        return Task.FromResult((IReadOnlyList<GpioPoint>)list);
    }

    /// <summary>
    /// Sets the requested GPO output on the specified device.
    /// </summary>
    /// <param name="dirno">The directory number that identifies the target device.</param>
    /// <param name="gpoId">The identifier of the GPO to update.</param>
    /// <param name="active"><see langword="true"/> to activate the output; otherwise, <see langword="false"/>.</param>
    /// <param name="timeSeconds">An optional duration, in seconds, for the output action.</param>
    /// <param name="ct">The cancellation token used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task SetGpoAsync(string dirno, string gpoId, bool active, int? timeSeconds, CancellationToken ct)
    {
        ThrowIfDisposed();

        if (string.IsNullOrEmpty(dirno))
            throw new ArgumentException("dirno must be provided.", "dirno");

        if (string.IsNullOrEmpty(gpoId))
            throw new ArgumentException("gpoId must be provided.", "gpoId");

        if (ct.IsCancellationRequested)
            return Task.FromCanceled(ct);

        string operation = active ? "set" : "clear";
        int time = timeSeconds ?? 0;

        _client.PostDeviceGPO(dirno, gpoId, operation, time);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Ensures realtime GPIO updates are subscribed for the specified device and callback.
    /// </summary>
    /// <param name="dirno">The directory number that identifies the target device.</param>
    /// <param name="onPoint">The callback that receives routed GPIO point updates.</param>
    public void EnsureSubscribed(string dirno, Action<GpioPoint> onPoint)
    {
        ThrowIfDisposed();

        if (string.IsNullOrEmpty(dirno))
            throw new ArgumentException("dirno must be provided.", "dirno");
        if (onPoint == null)
            throw new ArgumentNullException("onPoint");

        _callbacks[dirno] = onPoint;

        _client.TraceDeviceGPIStatusEvent();
        _client.TraceDeviceGPOStatusEvent();
    }

    /// <summary>
    /// Removes the registered callback for the specified device without disposing the transport itself.
    /// </summary>
    /// <param name="dirno">The directory number that identifies the target device.</param>
    public void DisposeFor(string dirno)
    {
        if (string.IsNullOrEmpty(dirno))
            return;

        Action<GpioPoint> removed;
        _callbacks.TryRemove(dirno, out removed);
    }

    /// <summary>
    /// Unsubscribes the transport from WAMP GPIO events and clears all registered callbacks.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        _client.OnWampDeviceGPIStatusEventEx -= OnGpiEventEx;
        _client.OnWampDeviceGPOStatusEventEx -= OnGpoEventEx;

        _callbacks.Clear();
    }

    private void OnGpiEventEx(object sender, WampGpioEventArgs e)
    {
        if (_disposed || e == null)
            return;

        Dispatch(e.Dirno, e.Element, GpioDirection.Gpi);
    }

    private void OnGpoEventEx(object sender, WampGpioEventArgs e)
    {
        if (_disposed || e == null)
            return;

        Dispatch(e.Dirno, e.Element, GpioDirection.Gpo);
    }

    private void Dispatch(string dirno, wamp_device_gpio_element element, GpioDirection direction)
    {
        if (_disposed || string.IsNullOrEmpty(dirno) || element == null)
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

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(WampGpioTransport));
    }

    private static GpioState ParseState(wamp_device_gpio_element e)
    {
        if (e == null)
            return GpioState.Inactive;

        if (!string.IsNullOrEmpty(e.state))
        {
            if (e.state.Equals("high", StringComparison.OrdinalIgnoreCase))
                return GpioState.Active;

            if (e.state.Equals("low", StringComparison.OrdinalIgnoreCase))
                return GpioState.Inactive;

            if (e.state == "1")
                return GpioState.Active;

            if (e.state == "0")
                return GpioState.Inactive;
        }

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
