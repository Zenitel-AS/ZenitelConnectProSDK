using ConnectPro.Enums;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static Wamp.Client.WampClient;

namespace ConnectPro.Models
{
    [Serializable]
    public class CallElement
    {
        public int CallId { get; set; }
        public CallType CallType { get; set; }
        public string FromDirno { get; set; }
        public string FromLegId { get; set; }
        public List<string> Groups { get; set; }
        public int Priority { get; set; }
        public int QueuePos { get; set; }
        public Reason Reason { get; set; }
        public DateTime StartTime { get; set; }
        public CallState CallState { get; set; }
        public string ToDirno { get; set; }
        public string ToDirnoCurrent { get; set; }

        public List<CallLegElement> CallLegs { get; set; }

        // Constructor for creating a CallElement from wamp_call_element
        public CallElement(wamp_call_element wampCallElement)
        {
            CallId = int.Parse(wampCallElement.call_id);
            FromDirno = wampCallElement.from_dirno;
            FromLegId = wampCallElement.from_leg_id;
            Priority = int.Parse(wampCallElement.priority);
            ToDirno = wampCallElement.to_dirno;
            ToDirnoCurrent = wampCallElement.to_dirno_current;

            if (Enum.TryParse(wampCallElement.call_type, ignoreCase: true, out CallType callType))
            {
                CallType = callType;
            }
            else
            {
                CallType = CallType.fault;
            }
            if (Enum.TryParse(wampCallElement.reason, ignoreCase: true, out Reason callreason))
            {
                Reason = callreason;
            }
            else
            {
                Reason = Reason.failure;
            }
            if (Enum.TryParse(wampCallElement.state, ignoreCase: true, out CallState callState))
            {
                CallState = callState;
            }
            else
            {
                CallState = CallState.fault;
            }

            CallLegs = new List<CallLegElement>();
        }

        // Method for adapting from wamp_call_element
        public static CallElement NewCallElementFromSdkCallElement(wamp_call_element wampCallElement)
        {
            return new CallElement(wampCallElement);
        }
    }
}
