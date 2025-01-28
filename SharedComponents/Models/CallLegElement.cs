using ConnectPro.Enums;
using ConnectPro.Interfaces;
using System;
using System.Collections.Generic;
using static Wamp.Client.WampClient;

namespace ConnectPro.Models
{
    [Serializable]
    public class CallLegElement
    {
        /// <summary>
        /// This list defines the directory number of the operators, who can answer a call in the queue. 
        /// </summary>
        public string call_id { get; set; }
        public CallType? call_type { get; set; }
        public string channel { get; set; }
        public string dirno { get; set; }
        public string name { get; set; }
        /// <summary>
        /// Defines the directory number of the calling device.
        /// </summary>
        public string from_dirno { get; set; }
        public string leg_id { get; set; }
        public string leg_role { get; set; }
        public string priority { get; set; }
        public Reason? reason { get; set; }

        public List<ICamera> Cameras { get; set; }
        /// <summary>
        /// Current state of the call in the queueu (join/leave)
        /// </summary>
        public CallState? state { get; set; }
        public string to_dirno { get; set; }

        public CallLegElement()
        {

        }

        public CallLegElement(wamp_call_leg_element sdk_element)
        {
            name = "";
            call_id = sdk_element.call_id;
            call_type = null;
            channel = sdk_element.channel;
            dirno = sdk_element.dirno;
            from_dirno = sdk_element.from_dirno;
            leg_id = sdk_element.leg_id;
            leg_role = sdk_element.leg_id;
            priority = sdk_element.priority;
            reason = null;
            state = null;
            to_dirno = sdk_element.to_dirno;
            if (Enum.TryParse(sdk_element.call_type, ignoreCase: true, out CallType callType))
            {
                call_type = callType;
            }
            else
            {
                call_type = null;
            }
            if (Enum.TryParse(sdk_element.reason, ignoreCase: true, out Reason callreason))
            {
                reason = callreason;
            }
            else
            {
                reason = null;
            }
            if (Enum.TryParse(sdk_element.state, ignoreCase: true, out CallState callState))
            {
                state = callState;
            }
            else
            {
                state = null;
            }
        }

        public static CallLegElement NewCallLegElementFromSdkElement(wamp_call_leg_element sdk_element)
        {
            return new CallLegElement(sdk_element);
        }
    }
}
