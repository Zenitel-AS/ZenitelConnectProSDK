using System;

namespace ConnectPro.Models.AccessControl
{
    public class OpenDoorEventData
    {
        /// <summary>
        /// Caller directory number (operator who opened the door).
        /// </summary>
        public string FromDirno { get; set; } = "";

        /// <summary>
        /// Directory number of the device having the door relay.
        /// </summary>
        public string DoorDirno { get; set; }

        /// <summary>
        /// Time when the door was opened (for logging purposes).
        /// </summary>
        public DateTime EventTime { get; private set; }

        /// <summary>
        /// Last event timestamp in Unix time (for rate-limiting, not stored permanently).
        /// Initialized to 0 (meaning "not set").
        /// </summary>
        public long LastEventTimestamp { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenDoorEventData"/> class.
        /// </summary>
        public OpenDoorEventData()
        {
            EventTime = DateTime.UtcNow;
            LastEventTimestamp = 0; // Indicates "not set"
        }

        /// <summary>
        /// Sets the last event timestamp for rate-limiting, but only once.
        /// </summary>
        public bool TrySetLastEventTimestamp()
        {
            if (LastEventTimestamp == 0) // If not set
            {
                LastEventTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                return true;
            }
            return false; // Prevents overwriting
        }
    }
}
