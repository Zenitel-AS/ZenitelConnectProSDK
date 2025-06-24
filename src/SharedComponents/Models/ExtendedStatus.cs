using Wamp.Client;

namespace ConnectPro.Models
{
    /// <summary>
    /// Represents the extended status of a WAMP device, including test results for tone and button tests.
    /// </summary>
    /// 
    public class ExtendedStatus
    {
        /// <summary>
        /// The current status of the test.
        /// Possible values: "passed", "failed", or other status indicators.
        /// </summary>
        public string CurrentStatus { get; set; }

        /// <summary>
        /// The unique identifier of the device.
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        /// The directory number of the device.
        /// </summary>
        public string Dirno { get; set; }

        /// <summary>
        /// A message detailing test results.
        /// Contains diagnostic information such as silence and tone levels.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// The timestamp of the last failed test in ISO 8601 format (UTC).
        /// Example: "2024-10-29T15:12:30.710373+00:00"
        /// </summary>
        public string LastFail { get; set; }

        /// <summary>
        /// The timestamp of the last successful test in ISO 8601 format (UTC).
        /// Example: "2024-10-29T15:12:23.70557+00:00"
        /// </summary>
        public string LastPass { get; set; }

        /// <summary>
        /// The timestamp when the test was last queued in ISO 8601 format (UTC).
        /// Example: "2024-10-29T15:12:22.062465+00:00"
        /// </summary>
        public string LastQueued { get; set; }

        /// <summary>
        /// Indicates whether there is a pending test.
        /// Possible values: "true" or "false".
        /// </summary>
        public string PendingTest { get; set; }

        /// <summary>
        /// The type of test being performed.
        /// Example: "tonetest".
        /// </summary>
        public string StatusType { get; set; }

        /// <summary>
        /// Initializes a new instance of the ExtendedStatus class.
        /// </summary>
        /// <param name="wamp_Device_Extended_Status">wamp class</param>
        public ExtendedStatus(WampClient.wamp_device_extended_status wamp_Device_Extended_Status)
        {
            CurrentStatus   = wamp_Device_Extended_Status.current_status;
            DeviceId        = wamp_Device_Extended_Status.device_id;
            Dirno           = wamp_Device_Extended_Status.dirno;
            ErrorMessage    = wamp_Device_Extended_Status.error_message;
            LastFail        = wamp_Device_Extended_Status.last_fail;
            LastPass        = wamp_Device_Extended_Status.last_pass;
            LastQueued      = wamp_Device_Extended_Status.last_queued;
            PendingTest     = wamp_Device_Extended_Status.pending_test;
            StatusType      = wamp_Device_Extended_Status.status_type;
        }
    }
}
