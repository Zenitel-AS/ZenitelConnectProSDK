using ConnectPro.Enums;
using System;

namespace ConnectPro.Models
{
    public sealed class GpioPoint
    {
        public int Id { get; }
        public GpioDirection Direction { get; }
        public GpioState State { get; }
        public DateTimeOffset UpdatedUtc { get; }
        public string RawState { get; }

        public GpioPoint(
            int id,
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
