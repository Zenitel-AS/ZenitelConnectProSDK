using ConnectPro.Models;
using ConnectPro.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wamp.Client;
using static Wamp.Client.WampClient;
using Timer = System.Timers.Timer;

namespace ConnectPro.Handlers
{
    /// <summary>
    /// Handles device management, including registration, retrieval, state updates, 
    /// and interaction with the WAMP client.
    /// </summary>
    
    public class DeviceHandler
    {
        #region Fields & Locks

        private Collections _collections;
        private Events _events;
        private WampClient _wamp;

        /// <summary>
        /// Synchronization lock for device retrieval operations.
        /// </summary>
        
        private object _lockObj = new object();
        
        /// <summary>
        /// Timer to periodically retrieve registered devices.
        /// </summary>
        
        private Timer DeviceRetrievalTimer { get; set; }

        #endregion

        #region Properties
       
        /// <summary>
        /// Indicates whether device retrieval is currently in progress.
        /// </summary>
        
        public bool IsExecutingDeviceRetrieval { get; set; } = false;
        
        /// <summary>
        /// Stores the IP address of the parent device.
        /// </summary>
        
        public string ParentIpAddress { get; set; } = "";

        #endregion

        #region Constructor & Initialization

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceHandler"/> class and registers event handlers.
        /// </summary>
        /// <param name="collections">Reference to the collections object managing registered devices.</param>
        /// <param name="events">Reference to the events object handling device-related events.</param>
        /// <param name="wamp">Reference to the WAMP client for communication.</param>
        /// <param name="parentIpAddress">Optional parent device IP address.</param>
       
        public DeviceHandler(ref Collections collections,
                             ref Events events,
                             ref WampClient wamp,
                             string parentIpAddress = "")
        {
            _collections = collections;
            _events = events;
            _wamp = wamp;
            ParentIpAddress = parentIpAddress;

            _wamp.OnWampDeviceRegistrationEvent += HandleDeviceRegistration;
            _events.OnDeviceRetrievalStart += HandleDeviceRetrievalStartEvent;
            _events.OnDeviceStateChange += HandleDeviceStateChange;
        }

        #endregion

        #region Event Handlers
        
        /// <summary>
        /// Handles device registration events from the WAMP client.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="ele">The device registration element.</param>
        
        private void HandleDeviceRegistration(object sender, WampClient.wamp_device_registration_element ele)
        {
            Device newDevice = new Device(ele); //Convert wamp sdk element to Device class

            if (!_collections.RegisteredDevices.Any(x => x.device_ip == ele.device_ip))
            {
                _collections.RegisteredDevices.Add(newDevice);
                _events.OnDeviceListChange?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                Device device = _collections.RegisteredDevices.FirstOrDefault(x => x.device_ip == ele.device_ip);
                if (device != null)
                {
                    _collections.RegisteredDevices.Remove(device);
                    _collections.RegisteredDevices.Add(newDevice);
                    _events.OnDeviceListChange?.Invoke(this, EventArgs.Empty);
                }
            }

        }
        
        /// <summary>
        /// Handles the start of the device retrieval process.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">Event arguments.</param>
        
        private void HandleDeviceRetrievalStartEvent(object sender, EventArgs e)
        {
            Task.Run(async () => await RetrieveRegisteredDevices());
        }
        
        /// <summary>
        /// Handles device state changes and updates the internal collections accordingly.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="callElement">The call element containing updated state information.</param>
        
        private void HandleDeviceStateChange(object sender, CallElement callElement)
        {
            try
            {
                Device device_to_dirno = _collections.RegisteredDevices.Where(x => x.dirno == callElement.ToDirno).FirstOrDefault();
                Device device_to_dirno_current = _collections.RegisteredDevices.Where(x => x.dirno == callElement.ToDirnoCurrent).FirstOrDefault();
                Device device_from_dirno = _collections.RegisteredDevices.Where(x => x.dirno == callElement.FromDirno).FirstOrDefault();

                if (device_to_dirno != null)
                    _collections.RegisteredDevices
                        .Where(x => x.dirno == callElement.ToDirno)
                        .First().CallState = callElement.CallState == Enums.CallState.ended ? Enums.CallState.reachable : callElement.CallState;

                if (device_to_dirno_current != null)
                    _collections.RegisteredDevices
                        .Where(x => x.dirno == callElement.ToDirnoCurrent)
                        .First().CallState = callElement.CallState == Enums.CallState.ended ? Enums.CallState.reachable : callElement.CallState;

                if (device_from_dirno != null)
                    _collections.RegisteredDevices
                        .Where(x => x.dirno == callElement.FromDirno)
                        .First().CallState = callElement.CallState == Enums.CallState.ended ? Enums.CallState.reachable : callElement.CallState;

                _events.OnDeviceListChange?.Invoke(this, EventArgs.Empty);

            }
            catch (Exception exe)
            {
                _events.OnExceptionThrown?.Invoke(this, exe);
            }
        }

        #endregion

        #region Device Retrieval & Management

        /// <summary>
        /// Retrieves the list of registered devices from the WAMP client and updates the collections.
        /// </summary>
        
        public async Task RetrieveRegisteredDevices()
        {
            await Task.Run(() =>
            {
                lock (_lockObj)
                {
                    try
                    {
                        if (!IsExecutingDeviceRetrieval)
                        {
                            IsExecutingDeviceRetrieval = true;
                            if (_wamp.IsConnected)
                            {
                                _collections.RegisteredDevices = GetRegisteredDevices().ToList();
                                _events.OnQueuesAndCallsSync?.Invoke(this, new EventArgs());

                                if (_collections.RegisteredDevices != null)
                                    DeviceRetrievalTimer?.Stop();

                                _events.OnDeviceListChange?.Invoke(this, new EventArgs());
                            }
                            IsExecutingDeviceRetrieval = false;
                        }
                        _events.OnDeviceRetrievalEnd?.Invoke(this, new EventArgs());
                    }
                    catch (Exception exe)
                    {
                        _events.OnDebugChanged?.Invoke(this, (exe.Message, exe));
                        _events.OnDeviceRetrievalEnd?.Invoke(this, new EventArgs());
                        _events.OnExceptionThrown?.Invoke(this, exe);
                    }
                }
            });
        }
        
        /// <summary>
        /// Retrieves a list of registered devices from the WAMP client.
        /// </summary>
        /// <returns>A collection of registered devices.</returns>
        
        public IEnumerable<Device> GetRegisteredDevices()
        {
            return ObjectConverter.ConvertSdkDeviceElementList(_wamp.requestRegisteredDevices().ToList());
        }

        #endregion

        #region Device Control Methods

        /// <summary>
        /// Simulates a key press on a device.
        /// </summary>
        /// <param name="deviceid">Device identifier (can be dirno or MAC address).</param>
        /// <param name="key">
        /// Specifies the key to be pressed. Examples:
        /// <list type="bullet">
        ///     <item><description><b>on, off</b> - Only relevant for <c>save-autoanswer</c>.</description></item>
        ///     <item><description><b>save-autoanswer</b> - Does not use press, tap, or release.</description></item>
        ///     <item><description><b>Set ID:</b> 
        ///         <c>m</c>, <c>c</c>, <c>p1..p2</c>, digits <c>[0-9]</c>, <c>save-autoanswer</c> 
        ///         (where <c>p1</c> corresponds to <c>dak0</c>).
        ///     </description></item>
        /// </list>
        /// </param>
        /// <param name="edge">The edge type.</param>
        /// <returns>True if the operation was successful, otherwise false.</returns>
        
        public bool SimulateKeyPress(string deviceid, string key, string edge)
        {
            wamp_response response = _wamp.PostDeviceIdKey("dirno="+deviceid, key,edge);
            if (response != null)
            {
                if (response.CompletionText == "PostKeyId sucessfully completed.")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Initiates a tone test on a specified device.
        /// </summary>
        /// <param name="dirno">Directory number of the device.</param>
        /// <param name="toneGroup">Tone group to test.</param>
        /// <returns>True if the operation was successful, otherwise false.</returns>
        
        public bool InitiateToneTest(string dirno, string toneGroup)
        {
            wamp_response response = _wamp.ToneTest(dirno, toneGroup);
            if (response != null)
            {
                if (response.CompletionText == "ToneTest sucessfully completed.")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        #endregion
    }
}
