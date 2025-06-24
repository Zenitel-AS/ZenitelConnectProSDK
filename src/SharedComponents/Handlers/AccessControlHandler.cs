using ConnectPro.Models;
using ConnectPro.Models.AccessControl;
using Microsoft.Extensions.Configuration;
using System;
using Wamp.Client;
using static Wamp.Client.WampClient;

namespace ConnectPro.Handlers
{
    /// <summary>
    /// Handles access control functionalities including opening doors.
    /// </summary>
    public class AccessControlHandler : IDisposable
    {
        Events _events;
        WampClient _wamp;
        Configuration _configuration;

        private readonly object _doorLock = new object(); // ✅ Shared lock object


        /// <summary>
        /// Gets or sets the IP address of the parent device.
        /// </summary>
        public string ParentIpAddress { get; set; } = "";
        /// <summary>
        /// Gets the event data for the last door opening event.
        /// </summary>
        public OpenDoorEventData OpenDoorEventData { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessControlHandler"/> class.
        /// </summary>
        /// <param name="events">Reference to the events handler.</param>
        /// <param name="wamp">Reference to the WAMP client.</param>
        /// <param name="configuration">Reference to the client configuration.</param>
        public AccessControlHandler(ref Events events, ref WampClient wamp, ref Configuration configuration)
        {
            _events = events;
            _wamp = wamp;
            _configuration = configuration;
            ParentIpAddress = _configuration.ServerAddr;

            _wamp.OnWampOpenDoorEvent += HandleWampOpenDoorEvent;
        }

        
        private void HandleWampOpenDoorEvent(object sender, WampClient.wamp_open_door_event doorOpenEvent)
        {
            lock (_doorLock) // ✅ Now all threads will wait for access
            {
                OpenDoorEventData = new OpenDoorEventData()
                {
                    FromDirno = doorOpenEvent.from_dirno,
                    DoorDirno = doorOpenEvent.door_dirno
                };
                OpenDoorEventData.TrySetLastEventTimestamp();

                Console.WriteLine($"Door opened! From dirno: {doorOpenEvent.from_dirno} Door dirno: {doorOpenEvent.door_dirno}");
                _events.OnDoorOpen?.Invoke(this, true);
            }
        }


        /// <summary>
        /// Opens the door associated with the specified device.
        /// </summary>
        /// <param name="ele">The device for which the door needs to be opened.</param>
        /// <param name="operatorDirNo">The device acting as an operator.</param>
        public void OpenDoor(Device ele, string operatorDirNo)
        {
            wamp_response response = _wamp.PostOpenDoor(ele.dirno);
            if (response != null)
            {
                if (response.CompletionText == "PostOpenDoor sucessfully completed.")
                {
                    OpenDoorEventData = new OpenDoorEventData()
                    {
                        FromDirno =  operatorDirNo,
                        DoorDirno = ele.dirno
                    };
                    OpenDoorEventData.TrySetLastEventTimestamp();

                    _events.OnDoorOpen?.Invoke(this, true);
                }
                else
                {
                    _events.OnDoorOpen?.Invoke(this, false);
                }
            }
        }

        #region IDisposable Implementation

        private bool _disposed = false;

        /// <summary>
        /// Disposes resources and unsubscribes from events.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Disposes resources and unsubscribes from events.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (_wamp != null)
                {
                    _wamp.OnWampOpenDoorEvent -= HandleWampOpenDoorEvent;
                }
            }

            _disposed = true;
        }

        #endregion

    }

}
