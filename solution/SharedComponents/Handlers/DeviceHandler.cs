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
    public class DeviceHandler
    {
        private Collections _collections;
        private Events _events;
        private WampClient _wamp;
        private object _lockObj = new object();

        private Timer DeviceRetrievalTimer { get; set; }

        public bool IsExecutingDeviceRetrieval { get; set; } = false;
        public string ParentIpAddress { get; set; } = "";

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
        private void HandleDeviceRetrievalStartEvent(object sender, EventArgs e)
        {
            Task.Run(async () => await RetrieveRegisteredDevices());
        }
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
        public IEnumerable<Device> GetRegisteredDevices()
        {
            return ObjectConverter.ConvertSdkDeviceElementList(_wamp.requestRegisteredDevices().ToList());
        }

        /// <summary>
        /// Simulate a key press on the device.
        /// </summary>
        /// <param name="deviceid">This segment may be either dirno or mac_address</param>
        /// <param name="key">Note:
        ///* on, off is only relevant for save-autoanswer
        ///* save-autoanswer does not use press, tap and release
        ///* set id: m, c , p1..p2, digits[0 - 9], save_autoanswer. (p1 correspondes to dak0)</param>
        /// <param name="edge"></param>
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
    }
}
