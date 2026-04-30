using ConnectPro.Enums;
using System;

namespace ConnectPro.Models
{
    /// <summary>
    /// Represents the state of a single GPIO input or output point.
    /// </summary>
    public sealed class GpioPoint
    {
        /// <summary>
        /// Gets the transport-specific identifier of the GPIO point.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the GPIO direction, such as input or output.
        /// </summary>
        public GpioDirection Direction { get; }

        /// <summary>
        /// Gets or sets the interpreted GPIO state.
        /// </summary>
        public GpioState State { get; set; }

        /// <summary>
        /// Gets the UTC timestamp when this GPIO point snapshot was created or updated.
        /// </summary>
        public DateTimeOffset UpdatedUtc { get; }

        /// <summary>
        /// Gets the raw state text received from the underlying transport.
        /// </summary>
        public string RawState { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GpioPoint"/> class.
        /// </summary>
        /// <param name="id">The GPIO point identifier.</param>
        /// <param name="direction">The GPIO point direction.</param>
        /// <param name="state">The interpreted GPIO state.</param>
        /// <param name="updatedUtc">The UTC timestamp associated with the point value.</param>
        /// <param name="rawState">The raw transport state value.</param>
        public GpioPoint(
            string id,
            GpioDirection direction,
            GpioState state,
            DateTimeOffset updatedUtc,
            string rawState)
        {
            Id = id;
            Direction = direction;
            State = state;
            UpdatedUtc = updatedUtc;
            RawState = rawState;
        }
    }
}
