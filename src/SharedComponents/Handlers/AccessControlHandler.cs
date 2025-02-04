using ConnectPro.Models;
using System;
using Wamp.Client;
using static Wamp.Client.WampClient;

namespace ConnectPro.Handlers
{
    /// <summary>
    /// Handles access control functionalities including opening doors.
    /// </summary>
    public class AccessControlHandler
    {
        Events _events;
        WampClient _wamp;

        /// <summary>
        /// Gets or sets the IP address of the parent device.
        /// </summary>
        public string ParentIpAddress { get; set; } = "";

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessControlHandler"/> class.
        /// </summary>
        /// <param name="events">Reference to the events handler.</param>
        /// <param name="wamp">Reference to the WAMP client.</param>
        /// <param name="parentIpAddress">IP address of the parent device.</param>
        public AccessControlHandler(ref Events events, ref WampClient wamp, string parentIpAddress)
        {
            _events = events;
            _wamp = wamp;

            _wamp.OnWampOpenDoorEvent += HandleWampOpenDoorEvent;
            ParentIpAddress = parentIpAddress;
        }

        private void HandleWampOpenDoorEvent(object sender, WampClient.wamp_open_door_event doorOpenEvent)
        {
            
        }

        /// <summary>
        /// Opens the door associated with the specified device.
        /// </summary>
        /// <param name="ele">The device for which the door needs to be opened.</param>
        public void OpenDoor(Device ele)
        {
            wamp_response response = _wamp.PostOpenDoor(ele.dirno);
            if (response != null)
            {
                if (response.CompletionText == "PostOpenDoor successfully completed.")
                {
                    _events.OnDoorOpen?.Invoke(this, true);
                }
                else
                {
                    _events.OnDoorOpen?.Invoke(this, false);
                }
            }
        }
    }

}
