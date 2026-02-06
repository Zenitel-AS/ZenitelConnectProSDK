using ConnectPro;
using ConnectPro.Models;
using SharedComponents.Models.GPIO;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wamp.Client;

/// <summary>
/// Composite transport that uses REST for snapshots and WAMP for realtime events.
/// </summary>
public sealed class HybridGpioTransport : IGpioTransport
{
    private readonly RestGpioTransport _rest;
    private readonly WampGpioTransport _wamp;

    public HybridGpioTransport(Core core, WampClient client)
    {
        if (core == null) throw new ArgumentNullException(nameof(core));
        if (client == null) throw new ArgumentNullException(nameof(client));

        _rest = new RestGpioTransport(core);
        _wamp = new WampGpioTransport(client);
    }

    public Task<IReadOnlyList<GpioPoint>> GetSnapshotAsync(string dirno, CancellationToken ct)
    {
        // Prefer REST snapshot to avoid WAMP argument issues
        return _rest.GetSnapshotAsync(dirno, ct);
    }

    public Task SetGpoAsync(string dirno, string gpoId, bool active, int? timeSeconds, CancellationToken ct)
    {
        // Prefer WAMP for actions (lower latency) and fallback to REST if it fails
        try
        {
            return _wamp.SetGpoAsync(dirno, gpoId, active, timeSeconds, ct);
        }
        catch
        {
            return _rest.SetGpoAsync(dirno, gpoId, active, timeSeconds, ct);
        }
    }

    public void EnsureSubscribed(string dirno, Action<GpioPoint> onPoint)
    {
        // Use WAMP subscriptions for realtime updates
        // WampGpioTransport expects Action<ConnectPro.Models.GpioPoint>
        _wamp.EnsureSubscribed(dirno, onPoint);
    }

    public void DisposeFor(string dirno)
    {
        _wamp.DisposeFor(dirno);
        _rest.DisposeFor(dirno);
    }
}
