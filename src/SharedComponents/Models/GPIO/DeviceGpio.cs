using ConnectPro.Enums;
using SharedComponents.Models.GPIO;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ConnectPro.Models.GPIO
{
    /// <summary>
    /// Represents the runtime GPIO view for a single device, including current inputs, outputs, and change notifications.
    /// </summary>
    public sealed class DeviceGpio : IDisposable
    {
        /// <summary>
        /// Gets the directory number of the device whose GPIO state is being tracked.
        /// </summary>
        public string Dirno { get; }

        /// <summary>
        /// Occurs when a GPIO point changes state or is first discovered.
        /// </summary>
        public event EventHandler<GpioChangedEventArgs> Changed;

        /// <summary>
        /// Gets the current collection of GPIO input points for the device.
        /// </summary>
        public IReadOnlyCollection<GpioPoint> Inputs => _gpis.Values;

        /// <summary>
        /// Gets the current collection of GPIO output points for the device.
        /// </summary>
        public IReadOnlyCollection<GpioPoint> Outputs => _gpos.Values;

        private readonly Dictionary<string, GpioPoint> _gpis = new Dictionary<string, GpioPoint>();
        private readonly Dictionary<string, GpioPoint> _gpos = new Dictionary<string, GpioPoint>();

        private readonly IGpioTransport _transport;

        // Tracks the initial refresh so consumers can await it if they want (optional).
        private readonly Task _initialRefreshTask;

        internal DeviceGpio(string dirno, IGpioTransport transport)
        {
            if (string.IsNullOrWhiteSpace(dirno))
                throw new ArgumentException("dirno must be provided.", nameof(dirno));

            Dirno = dirno;
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));

            // 1) Start listening immediately (per your requirement).
            //    Any live change events will route through Upsert -> Changed event.
            _transport.EnsureSubscribed(Dirno, Upsert);

            // 2) Immediately load current state snapshot (fire-and-forget, but safe).
            //    We intentionally do not block construction.
            _initialRefreshTask = SafeInitialRefresh();
        }

        /// <summary>
        /// Optional: allows callers to await the first snapshot load (useful for UI/tests).
        /// </summary>
        /// <returns>A task that completes when the initial GPIO snapshot has been loaded.</returns>
        public Task WhenInitializedAsync() => _initialRefreshTask;

        /// <summary>
        /// Refreshes the current GPIO snapshot for the device from the underlying transport.
        /// </summary>
        /// <param name="ct">The cancellation token used to cancel the refresh operation.</param>
        /// <returns>A task that represents the asynchronous refresh operation.</returns>
        public async Task RefreshAsync(CancellationToken ct)
        {
            var points = await _transport.GetSnapshotAsync(Dirno, ct).ConfigureAwait(false);

            foreach (var point in points)
            {
                Upsert(point);
            }
        }

        /// <summary>
        /// Activates the specified GPIO output.
        /// </summary>
        /// <param name="gpoId">The identifier of the GPIO output to activate.</param>
        /// <param name="timeSeconds">An optional activation duration, in seconds.</param>
        /// <param name="ct">The cancellation token used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous activation operation.</returns>
        public Task ActivateAsync(string gpoId, int? timeSeconds, CancellationToken ct)
            => SetAsync(gpoId, true, timeSeconds, ct);

        /// <summary>
        /// Deactivates the specified GPIO output.
        /// </summary>
        /// <param name="gpoId">The identifier of the GPIO output to deactivate.</param>
        /// <param name="ct">The cancellation token used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous deactivation operation.</returns>
        public Task DeactivateAsync(string gpoId, CancellationToken ct)
            => SetAsync(gpoId, false, null, ct);

        private async Task SetAsync(string gpoId, bool active, int? timeSeconds, CancellationToken ct)
        {
            await _transport.SetGpoAsync(Dirno, gpoId, active, timeSeconds, ct)
                            .ConfigureAwait(false);
        }

        internal void Upsert(GpioPoint point)
        {
            var dict = point.Direction == GpioDirection.Gpi ? _gpis : _gpos;

            if (dict.TryGetValue(point.Id, out var existing))
            {
                if (existing.State == point.State)
                    return;
            }

            dict[point.Id] = point;

            Changed?.Invoke(this, new GpioChangedEventArgs(Dirno, point));
        }

        private async Task SafeInitialRefresh()
        {
            try
            {
                // No cancellation: this is the "startup fill" intended to happen always.
                await RefreshAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch
            {
                // Intentionally swallow: "start listening immediately" must not crash the process
                // if snapshot fails (e.g., temporary connection issue). Live events can still arrive.
                // If you want logging, inject a logger into DeviceGpio later.
            }
        }

        /// <summary>
        /// Stops routing GPIO updates for this device through the underlying transport.
        /// </summary>
        public void Dispose()
        {
            _transport.DisposeFor(Dirno);
        }
    }
}
