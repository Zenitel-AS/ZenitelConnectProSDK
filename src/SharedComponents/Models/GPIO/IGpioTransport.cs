using ConnectPro.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SharedComponents.Models.GPIO
{
    /// <summary>
    /// Transport abstraction for device GPIO operations.
    /// Implementations typically use WAMP (ZCP) but the model remains transport-agnostic.
    /// </summary>
    public interface IGpioTransport
    {
        /// <summary>
        /// Gets a full snapshot of currently known GPIO points (both GPI and GPO) for a device.
        /// </summary>
        Task<IReadOnlyList<GpioPoint>> GetSnapshotAsync(string dirno, CancellationToken ct);

        /// <summary>
        /// Sets a GPO output state. "active=true" activates, "false" deactivates.
        /// timeSeconds is optional (when supported by backend).
        /// </summary>
        Task SetGpoAsync(string dirno, string gpoId, bool active, int? timeSeconds, CancellationToken ct);

        /// <summary>
        /// Ensures the transport is subscribed to GPIO change events for the specified device.
        /// The callback must be invoked on each point update.
        /// </summary>
        void EnsureSubscribed(string dirno, Action<GpioPoint> onPoint);

        /// <summary>
        /// Disposes/unsubscribes resources for a specific device (idempotent).
        /// </summary>
        void DisposeFor(string dirno);
    }
}
