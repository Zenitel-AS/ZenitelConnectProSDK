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
public sealed class HybridGpioTransport : IGpioTransport, IDisposable
{
    private readonly RestGpioTransport _rest;
    private readonly WampGpioTransport _wamp;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="HybridGpioTransport"/> class with the specified core and WAMP
    /// client.
    /// </summary>
    /// <param name="core">The core instance used to initialize the REST GPIO transport.</param>
    /// <param name="client">The WAMP client used to initialize the WAMP GPIO transport.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="core"/> or <paramref name="client"/> is <see langword="null"/>.</exception>
    public HybridGpioTransport(Core core, WampClient client)
    {
        if (core == null) throw new ArgumentNullException(nameof(core));
        if (client == null) throw new ArgumentNullException(nameof(client));

        _rest = new RestGpioTransport(core);
        _wamp = new WampGpioTransport(client);
    }

    /// <summary>
    /// Retrieves a snapshot of GPIO points for the specified directory.
    /// </summary>
    /// <param name="dirno">The directory number identifier.</param>
    /// <param name="ct">The cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only list of GPIO points.</returns>
    public Task<IReadOnlyList<GpioPoint>> GetSnapshotAsync(string dirno, CancellationToken ct)
    {
        ThrowIfDisposed();
        return _rest.GetSnapshotAsync(dirno, ct);
    }

    /// <summary>
    /// Sets the state of a General Purpose Output (GPO) asynchronously, attempting WAMP first then falling back to
    /// REST.
    /// </summary>
    /// <param name="dirno">The directory number or device identifier.</param>
    /// <param name="gpoId">The GPO identifier.</param>
    /// <param name="active">True to activate the GPO; false to deactivate it.</param>
    /// <param name="timeSeconds">The optional duration in seconds for the GPO state, or null for indefinite duration.</param>
    /// <param name="ct">The cancellation token to observe.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task SetGpoAsync(string dirno, string gpoId, bool active, int? timeSeconds, CancellationToken ct)
    {
        ThrowIfDisposed();

        try
        {
            return _wamp.SetGpoAsync(dirno, gpoId, active, timeSeconds, ct);
        }
        catch
        {
            return _rest.SetGpoAsync(dirno, gpoId, active, timeSeconds, ct);
        }
    }
    
    /// <summary>
    /// Ensures that the specified directory is subscribed to GPIO point updates.
    /// </summary>
    /// <param name="dirno">The directory number or device identifier.</param>
    /// <param name="onPoint">The action to invoke when a GPIO point update is received.</param>
    public void EnsureSubscribed(string dirno, Action<GpioPoint> onPoint)
    {
        ThrowIfDisposed();
        _wamp.EnsureSubscribed(dirno, onPoint);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dirno"></param>
    public void DisposeFor(string dirno)
    {
        if (_disposed)
            return;

        _wamp.DisposeFor(dirno);
        _rest.DisposeFor(dirno);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        _wamp.Dispose();
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(HybridGpioTransport));
    }
}
