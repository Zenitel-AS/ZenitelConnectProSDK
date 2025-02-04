using ConnectPro.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Wamp.Client.WampClient;

namespace ConnectPro.Debug
{
    /// <summary>
    /// Handles logging of call events, including call initiation, queued calls, 
    /// and door events. Uses an internal device lookup for efficient access.
    /// </summary>
    public class Log
    {
        #region Fields

        private readonly Collections _collections;
        private readonly Events _events;

        /// <summary>
        /// Dictionary for quick lookup of registered devices by their directory numbers.
        /// </summary>
        
        private readonly ConcurrentDictionary<string, Device> _deviceLookup;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Log"/> class.
        /// </summary>
        /// <param name="collections">Reference to the collections object containing registered devices.</param>
        /// <param name="events">Reference to the events object handling log events.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="collections"/> or <paramref name="events"/> is null.</exception>
        public Log(Collections collections, Events events)
        {
            _collections = collections ?? throw new ArgumentNullException(nameof(collections));
            _events = events ?? throw new ArgumentNullException(nameof(events));

            _deviceLookup = new ConcurrentDictionary<string, Device>();

            // Initialize the device lookup for faster access
            _events.OnDeviceRetrievalEnd += SetDeviceLookupDictionary;
            _events.OnLogEntryRequested += async (sender, info) => await RecordCallLogAsync(info);
        }

        #endregion

        #region Device Lookup

        /// <summary>
        /// Populates the device lookup dictionary from the registered devices collection.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        
        private void SetDeviceLookupDictionary(object sender, EventArgs e)
        {
            foreach (var device in _collections.RegisteredDevices)
            {
                _deviceLookup.TryAdd(device.dirno, device);
            }
        }

        /// <summary>
        /// Retrieves a device from the lookup dictionary based on the directory number.
        /// </summary>
        /// <param name="dirno">Directory number of the device.</param>
        /// <returns>The matching <see cref="Device"/> object.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the device is not found.</exception>
        
        private Device GetDeviceByDirno(string dirno)
        {
            if (_deviceLookup.TryGetValue(dirno, out var device))
            {
                return device;
            }
            throw new InvalidOperationException($"Device with dirno {dirno} not found.");
        }

        #endregion

        #region Logging Methods

        /// <summary>
        /// Asynchronously records a call log event.
        /// </summary>
        /// <param name="call_info">Call information object.</param>
        
        public async Task RecordCallLogAsync(object call_info)
        {
            try
            {
                if (_deviceLookup == null || _deviceLookup.Count == 0)
                {
                    throw new InvalidOperationException("No registered devices available.");
                }

                CallLog logEntry = CreateLogEntry(call_info);

                await Task.Run(() => _events.OnLogEntryAdded?.Invoke(this, logEntry));
            }
            catch (Exception ex)
            {
                await Task.Run(() => _events.OnExceptionThrown?.Invoke(this, ex));
            }
        }

        /// <summary>
        /// Creates a log entry from the given call information.
        /// </summary>
        /// <param name="call_info">Call information object.</param>
        /// <returns>A populated <see cref="CallLog"/> entry.</returns>
        /// <exception cref="ArgumentException">Thrown if the call_info type is not recognized.</exception>
        
        private CallLog CreateLogEntry(object call_info)
        {
            if (call_info is wamp_call_element wampCallElement)
            {
                var senderDevice = GetDeviceByDirno(wampCallElement.from_dirno);
                return PopulateLogEntry(senderDevice, wampCallElement.from_dirno, wampCallElement.to_dirno,
                                        wampCallElement.to_dirno_current, "Call", wampCallElement.state, wampCallElement.reason);
            }
            else if (call_info is wamp_call_leg_element callLegElement)
            {
                var senderDevice = GetDeviceByDirno(callLegElement.from_dirno);
                return PopulateLogEntry(senderDevice, callLegElement.from_dirno, callLegElement.to_dirno,
                                        "", "Queue", callLegElement.state, callLegElement.reason);
            }
            else if (call_info is CallElement callElement)
            {
                var senderDevice = GetDeviceByDirno(callElement.FromDirno);
                return PopulateLogEntry(senderDevice, callElement.FromDirno, callElement.ToDirno,
                                        callElement.ToDirnoCurrent, "Call", callElement.CallState.ToString(), callElement?.Reason.ToString());
            }
            else
            {
                throw new ArgumentException("Invalid call information object type.", nameof(call_info));
            }
        }

        /// <summary>
        /// Populates a <see cref="CallLog"/> entry with relevant details.
        /// </summary>
        /// <param name="senderDevice">The originating device.</param>
        /// <param name="fromDirno">The directory number of the sender.</param>
        /// <param name="toDirno">The directory number of the receiver.</param>
        /// <param name="answeredByDirno">The directory number of the device that answered the call.</param>
        /// <param name="callType">The type of call.</param>
        /// <param name="state">The state of the call.</param>
        /// <param name="reason">The reason for the call event.</param>
        /// <returns>A populated <see cref="CallLog"/> entry.</returns>
        
        private CallLog PopulateLogEntry(Device senderDevice, string fromDirno, string toDirno, string answeredByDirno, string callType, string state, string reason)
        {
            return new CallLog
            {
                Time = DateTime.UtcNow,
                DeviceName = senderDevice?.name ?? "Unknown Device",
                FromDirno = fromDirno,
                ToDirno = toDirno,
                AnsweredByDirno = answeredByDirno,
                CallType = callType,
                State = GetDescriptiveState(state),
                Reason = reason,
                sender = senderDevice,
                Location = senderDevice?.location ?? "Unknown Location"
            };
        }

        /// <summary>
        /// Asynchronously records a door event log entry.
        /// </summary>
        /// <param name="from">The originating device.</param>
        /// <param name="to">The receiving device.</param>
        
        public async Task RecordDoorEventAsync(Device from, Device to)
        {
            try
            {
                var logEntry = new CallLog
                {
                    Time = DateTime.UtcNow,
                    DeviceName = from.name,
                    FromDirno = from.dirno,
                    ToDirno = to.dirno,
                    AnsweredByDirno = to.dirno,
                    CallType = "Call",
                    State = "Door open",
                    Reason = "Door open",
                    sender = from,
                    Location = from.location
                };

                await Task.Run(() => _events.OnLogEntryAdded?.Invoke(this, logEntry));
            }
            catch (Exception ex)
            {
                await Task.Run(() => _events.OnExceptionThrown?.Invoke(this, ex));
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Converts call states to human-readable descriptions.
        /// </summary>
        /// <param name="state">The call state string.</param>
        /// <returns>A descriptive string representing the call state.</returns>
        
        private string GetDescriptiveState(string state)
        {
            switch (state)
            {
                case "in_call":
                    return "In Call";
                case "ringing":
                    return "Ringing";
                case "queued":
                    return "Queued";
                case "ended":
                    return "Ended";
                default:
                    return "Unknown";
            }
        }

        #endregion
    }
}
