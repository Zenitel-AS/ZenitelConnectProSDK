using ConnectPro.Enums;
using SharedComponents.Models.GPIO;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ConnectPro.Models.GPIO
{
    public sealed class DeviceGpio : IDisposable
    {
        public string Dirno { get; }

        public event EventHandler<GpioChangedEventArgs> Changed;

        public IReadOnlyCollection<GpioPoint> Inputs => _gpis.Values;

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
        public Task WhenInitializedAsync() => _initialRefreshTask;

        public async Task RefreshAsync(CancellationToken ct)
        {
            var points = await _transport.GetSnapshotAsync(Dirno, ct).ConfigureAwait(false);

            foreach (var point in points)
            {
                Upsert(point);
            }
        }

        public Task ActivateAsync(string gpoId, int? timeSeconds, CancellationToken ct)
            => SetAsync(gpoId, true, timeSeconds, ct);

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

        public void Dispose()
        {
            _transport.DisposeFor(Dirno);
        }
    }
}
