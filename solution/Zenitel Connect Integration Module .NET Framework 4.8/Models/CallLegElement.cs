using System.Collections.Generic;
using static Wamp.Client.WampClient;

namespace ConnectPro.Models
{
    public class CallLegElement
    {
        /// <summary>
        /// This list defines the directory number of the operators, who can answer a call in the queue. 
        /// </summary>
        public string call_id { get; set; }
        public string call_type { get; set; }
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
        public string reason { get; set; }

        public List<Camera> Cameras { get; set; }
        /// <summary>
        /// Current state of the call in the queueu (join/leave)
        /// </summary>
        public string state { get; set; }
        public string to_dirno { get; set; }

        public static CallLegElement NewCallLegElementFromSdkElement(wamp_call_leg_element sdk_element)
        {
            return new CallLegElement()
            {
                name = "",
                call_id = sdk_element.call_id,
                call_type = sdk_element.call_type,
                channel = sdk_element.channel,
                dirno = sdk_element.dirno,
                from_dirno = sdk_element.from_dirno,
                leg_id = sdk_element.leg_id,
                leg_role = sdk_element.leg_id,
                priority = sdk_element.priority,
                reason = sdk_element.reason,
                state = sdk_element.state,
                to_dirno = sdk_element.to_dirno
            };
        }


    }
}
