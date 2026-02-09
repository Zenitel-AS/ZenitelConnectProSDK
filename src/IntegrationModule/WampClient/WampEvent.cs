using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wamp.Client
{

    public partial class WampClient
    {

        /// <summary>This class contains device registration element.</summary>
        public class wamp_device_registration_element
        {
            /// <summary>
            /// Device_ip is the IP-Address of the device.
            /// </summary>
            public string device_ip { get; set; }

            /// <summary>
            /// Device_type is the HW-type of the device.
            /// </summary>
            public string device_type { get; set; }

            /// <summary>
            /// Dirno is the directory number of the device.
            /// </summary>
            public string dirno { get; set; }

            /// <summary>
            /// Location is the where the device is placed.
            /// </summary>
            public string location { get; set; }

            /// <summary>
            /// Name is the assigned name of the device.
            /// </summary>
            public string name { get; set; }

            /// <summary>
            /// State indicates if the device is "reachable" or "not reachable".
            /// </summary>
            public string state { get; set; }
        }


        /// <summary>
        /// This class incapsulates the call element
        /// </summary>
        public class wamp_call_element
        {
            /// <summary>
            /// This attribute defines the unique Identification of the call 
            /// </summary>
            public string call_id { get; set; }

            /// <summary>
            /// This attribute defines the unique Identification of the call queueu the call is a member of 
            /// </summary>
            public string call_queueid { get; set; }

            /// <summary>
            /// This attribute defines the type of the call 
            /// </summary>
            public string call_type { get; set; }

            /// <summary>
            /// This attribute defines the caller device of the call 
            /// </summary>
            public string from_dirno { get; set; }

            /// <summary>
            /// This attribute defines the caller device of the call 
            /// </summary>
            public string from_leg_id { get; set; }

            /// <summary>
            /// This attribute defines priority of the call 
            /// </summary>
            public string priority { get; set; }

            /// <summary>
            /// This attribute defines the reason for the call event 
            /// </summary>
            public string reason { get; set; }

            /// <summary>
            /// This attribute defines the calling state of the call 
            /// </summary>
            public string state { get; set; }

            /// <summary>
            /// This attribute defines the called device of the call 
            /// </summary>
            public string to_dirno { get; set; }

            /// <summary>
            /// This attribute defines the called device of the call 
            /// </summary>
            public string to_dirno_current { get; set; }
        }


        /// <summary>
        /// This class defines a call leg
        /// </summary>
        public class wamp_call_leg_element
        {
            /// <summary>
            /// This list defines the directory number of the operators, who can answer a call in the queue. 
            /// </summary>
            public string call_id { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string call_type { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string channel { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string dirno { get; set; }

            /// <summary>
            /// Defines the directory number of the calling device.
            /// </summary>
            public string from_dirno { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string leg_id { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string leg_role { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string priority { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string reason { get; set; }

            /// <summary>
            /// Current state of the call in the queueu (join/leave)
            /// </summary>
            public string state { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string to_dirno { get; set; }
        }


        /// <summary>
        /// This class encapsulates the address information of the Interface List request.
        /// </summary>
        public class addrInfo
        {
            /// <summary>
            /// IP-Net Broadcast address.
            /// </summary>
            public string broadcast { get; set; }

            /// <summary>
            /// Family name.
            /// </summary>
            public string family { get; set; }

            /// <summary>
            /// Name of the IP port
            /// </summary>
            public string label { get; set; }

            /// <summary>
            /// IP address of the port.
            /// </summary>
            public string local { get; set; }

            /// <summary>
            /// TBD
            /// </summary>
            public string preferred_life_time { get; set; }

            /// <summary>
            /// TBD
            /// </summary>
            public string prefixlen { get; set; }

            /// <summary>
            /// Globel/Link
            /// </summary>
            public string scope { get; set; }

            /// <summary>
            /// TBD
            /// </summary>
            public string valid_life_time { get; set; }
        }


        /// <summary>
        /// This class encapsulates the IP Interface Status
        /// </summary>
        public class wamp_interface_list
        {
            /// <summary>
            ///  List of available WAMP connections
            /// </summary>
            public List<addrInfo> addr_info { get; set; }

            /// <summary>
            /// MAC-address of the IP port
            /// </summary>
            public string address { get; set; }

            /// <summary>
            /// TBD
            /// </summary>
            public string broadcast { get; set; }

            /// <summary>
            /// TBD
            /// </summary>
            public List<string> flags { get; set; }

            /// <summary>
            /// TBD
            /// </summary>
            public string group { get; set; }

            /// <summary>
            /// TBD
            /// </summary>
            public int ifindex { get; set; }

            /// <summary>
            /// Interface name
            /// </summary>
            public string ifname { get; set; }

            /// <summary>
            /// Type of link (ether")
            /// </summary>
            public string link_type { get; set; }

            /// <summary>
            /// TBD
            /// </summary>
            public int mtu { get; set; }

            /// <summary>
            /// Operational state (UP/DOWN)
            /// </summary>
            public string operstate { get; set; }

            /// <summary>
            /// TBD
            /// </summary>
            public string qdisc { get; set; }

            /// <summary>
            /// TBD
            /// </summary>
            public int txqlen { get; set; }
        }


        /// <summary>
        /// Class encapsulating the General Purpose I/O element
        /// </summary>
        public class wamp_device_gpio_element
        {
            /// <summary>
            /// Identity of the GPO. The values depend on what the hardware supports and the device configuration. Examples: relay1, gpi4, e_relay1, gpo1
            /// </summary>
            public string id { get; set; }

            /// <summary>
            /// The state of the GPO (low/High).
            /// </summary>
            public string state { get; set; }

            /// <summary>
            /// The type of GPO.
            /// </summary>
            public string type { get; set; }

            /// <summary>
            /// The operation performed on the GPO (set/clear). Present in GPO status events when <see cref="state"/> is not provided.
            /// </summary>
            public string operation { get; set; }

            /// <summary>
            /// The relays bitmask value. Present in GPO status events (e.g. "0" or "1").
            /// </summary>
            public string relays { get; set; }

            /// <summary>
            /// The directory number of the device. Present in GPIO event payloads.
            /// </summary>
            public string dirno { get; set; }
        }


        /// <summary>
        /// This class encapsulates the Open Door Event
        /// </summary>
        public class wamp_open_door_event
        {
            /// <summary>
            /// Caller directory number.
            /// </summary>
            public string from_dirno { get; set; }

            /// <summary>
            /// Name of the caller.
            /// </summary>
            public string from_name { get; set; }

            /// <summary>
            /// Directory number of the device having the door relay.
            /// </summary>
            public string door_dirno { get; set; }

            /// <summary>
            /// Name of the device having the door relay.
            /// </summary>
            public string door_name { get; set; }
        }

        /// <summary>
        /// The Zenitel Connect Pro software version 
        /// </summary>
        public class wamp_platform_version
        {
            /// <summary>
            /// Zenitel Connect Pro Software Version.
            /// </summary>
            public string version { get; set; }
        }

        /// <summary>
        /// These enums define the response possible received from the WAMP connection when sending a request
        /// </summary>
        public enum ResponseType
        {
            /// <summary>No Response received from WAMP Connection.</summary>
            WampNoResponce,

            /// <summary>A negative response received from WAMP Connection.</summary>
            WampRequestFailed,

            /// <summary>A positive response received from WAMP Connection.</summary>
            WampRequestSucceeded
        }

        /// <summary>
        /// This class defines the response received from the WAMP connection when sending a request.
        /// </summary>
        public class wamp_response
        {
            /// <summary>
            /// Contains the result of the WAMP request (no response, failed, success)
            /// </summary>
            public ResponseType WampResponse { get; set; }

            /// <summary>
            /// Contains additional information of the request completion
            /// </summary>
            public string CompletionText { get; set; }

            /// <summary>
            /// WAMP Response creator
            /// </summary>
            public wamp_response()
            {
                WampResponse = ResponseType.WampNoResponce;
                CompletionText = "";
            }
        }


        /// <summary>
        /// WAMP group definition
        /// </summary>
        public class wamp_group_element
        {
            /// <summary>
            /// Directory number of the group.
            /// </summary>
            public string dirno { get; set; }

            /// <summary>
            /// Name of the group.
            /// </summary>
            public string displayname { get; set; }

            /// <summary>
            /// Priority of the group.
            /// </summary>
            public string priority { get; set; }

            /// <summary>
            /// List of all members the group consists of.
            /// </summary>
            public string[] members { get; set; }
        }


        /// <summary>
        /// Class that wraps all configured audio messages.
        /// </summary>
        public class AudioMessageWrapper
        {
            /// <summary>
            /// The class wraps all configured audio messages.
            /// </summary>
            /// <returns>A list of all configured audio messages.</returns>
            [JsonProperty("audio_messages")]
            public List<wamp_audio_messages_element> AudioMessages { get; set; }

            /// <summary>
            /// The method returns the available space left for audio messages.
            /// </summary>
            /// <returns>Available stores space for audio messages.</returns>
            [JsonProperty("available_space")]
            public int AvailableSpace { get; set; }

            /// <summary>
            /// The method returns the space occupied by the configured audio messages.
            /// </summary>
            /// <returns>Space occupied by the configured audio message.</returns>
            [JsonProperty("used_space")]
            public int? UsedSpace { get; set; }
        }


        /// <summary>
        /// Class that defines defines an audio message.
        /// </summary>
        public class wamp_audio_messages_element
        {
            /// <summary>
            /// Directory number of the audio message.
            /// </summary>
            public string dirno { get; set; }

            /// <summary>
            /// File name of the audio message.
            /// </summary>
            public string filename { get; set; }


            /// <summary>
            /// File path of the audio message.
            /// </summary>
            public string filepath { get; set; }

            /// <summary>
            /// File size of the audio message.
            /// </summary>
            public int filesize { get; set; }

            /// <summary>
            /// Time used for replaying the audio message.
            /// </summary>
            public int duration { get; set; }
        }


        /// <summary>
        /// Class that defines directory number as a digital number and a name.
        /// </summary>
        public class wamp_directory_number_element
        {
            /// <summary>
            /// The directory number of the element
            /// </summary>
            public string dirno { get; set; }

            /// <summary>
            /// The name of the element
            /// </summary>
            public string displayname { get; set; }
        }


        /// <summary>  </summary>
        public class wamp_audio_event_detection
        {
            /// <summary> </summary>
            public string from_dirno { get; set; }

            /// <summary> </summary>
            public string from_name { get; set; }

            /// <summary> </summary>
            public string audio_event { get; set; }

            /// <summary> </summary>
            public string probability { get; set; }

            /// <summary> </summary>
            public string UCT_time { get; set; }
        }


            /// <summary> </summary>
            public class wamp_audio_data_receiving
        {
            /// <summary> </summary>
            public string from_dirno { get; set; }

            /// <summary> </summary>
            public string from_name { get; set; }

            /// <summary> </summary>
            public string status { get; set; }

            /// <summary> </summary>
            public string UCT_time { get; set; }
        }


        /// <summary> </summary>
        public class wamp_audio_detector_alive
        {
            /// <summary> </summary>
            public string from_dirno { get; set; }

            /// <summary> </summary>
            public string from_name { get; set; }

            /// <summary> </summary>
            public string UCT_time { get; set; }
        }

        /// <summary>
        /// Represents the extended status of a WAMP device, including test results for tone and button tests.
        /// </summary>
        public class wamp_device_extended_status
        {
            /// <summary>
            /// The current status of the test.
            /// Possible values: "passed", "failed", or other status indicators.
            /// </summary>
            public string current_status { get; set; }

            /// <summary>
            /// The unique identifier of the device.
            /// </summary>
            public int device_id { get; set; }

            /// <summary>
            /// The directory number of the device.
            /// </summary>
            public string dirno { get; set; }

            /// <summary>
            /// A message detailing test results.
            /// Contains diagnostic information such as silence and tone levels.
            /// </summary>
            public string error_message { get; set; }

            /// <summary>
            /// The timestamp of the last failed test in ISO 8601 format (UTC).
            /// Example: "2024-10-29T15:12:30.710373+00:00"
            /// </summary>
            public string last_fail { get; set; }

            /// <summary>
            /// The timestamp of the last successful test in ISO 8601 format (UTC).
            /// Example: "2024-10-29T15:12:23.70557+00:00"
            /// </summary>
            public string last_pass { get; set; }

            /// <summary>
            /// The timestamp when the test was last queued in ISO 8601 format (UTC).
            /// Example: "2024-10-29T15:12:22.062465+00:00"
            /// </summary>
            public string last_queued { get; set; }

            /// <summary>
            /// Indicates whether there is a pending test.
            /// Possible values: "true" or "false".
            /// </summary>
            public string pending_test { get; set; }

            /// <summary>
            /// The type of test being performed.
            /// Example: "tonetest".
            /// </summary>
            public string status_type { get; set; }
        }
        // Inside: namespace Wamp.Client { public partial class WampClient { ... } }

        /// <summary>
        /// Operator element used by call_queue. Schema shape: { "dirno": "112" }
        /// </summary>
        public class wamp_operator_element
        {
            /// <summary>
            /// Directory number of operator.
            /// </summary>
            public string dirno { get; set; }
        }

        /// <summary>
        /// Rich call element for call queues.
        /// Schema: call_basic + call_legs[]
        /// </summary>
        public class wamp_call_rich_element : wamp_call_element
        {
            /// <summary>
            /// The active legs of the call. Caller leg first.
            /// </summary>
            public List<wamp_call_leg_element> call_legs { get; set; }
        }

        /// <summary>
        /// Call queue schema:
        /// - queue_dirno
        /// - calls[]
        /// - operators[]
        /// </summary>
        public class wamp_call_queue_element
        {
            /// <summary>
            /// The directory number of the call queue.
            /// </summary>
            public string queue_dirno { get; set; }

            /// <summary>
            /// Calls queued in the call queue. Listed in order, first in queue first.
            /// </summary>
            public List<wamp_call_rich_element> calls { get; set; }

            /// <summary>
            /// Operators of the call queue.
            /// </summary>
            public List<wamp_operator_element> operators { get; set; }
        }

        /// <summary>
        /// This class encapsulates a call forwarding rule element as returned by the API.
        /// </summary>
        public class wamp_call_forwarding_element
        {
            /// <summary>
            /// The directory number that owns this forwarding rule.
            /// </summary>
            public string dirno { get; set; }

            /// <summary>
            /// The type of forwarding: "unconditional", "on_busy", or "on_timeout".
            /// </summary>
            public string fwd_type { get; set; }

            /// <summary>
            /// The directory number to forward calls to.
            /// </summary>
            public string fwd_to { get; set; }

            /// <summary>
            /// Indicates whether the forwarding rule is enabled.
            /// </summary>
            public bool enabled { get; set; }
        }

        /// <summary>
        /// This class defines the response from a POST to the call forwarding API.
        /// </summary>
        public class wamp_call_forwarding_post_response
        {
            /// <summary>
            /// The number of input entries that failed validation.
            /// </summary>
            public int error_cnt { get; set; }

            /// <summary>
            /// The number of entries that were successfully updated or added.
            /// </summary>
            public int update_cnt { get; set; }

            /// <summary>
            /// List of error descriptions, if any.
            /// </summary>
            public List<string> errors { get; set; }
        }

        /// <summary>
        /// This class defines the response from a DELETE to the call forwarding API.
        /// </summary>
        public class wamp_call_forwarding_delete_response
        {
            /// <summary>
            /// The number of entries that were deleted.
            /// </summary>
            public int delete_cnt { get; set; }
        }

    }
}
