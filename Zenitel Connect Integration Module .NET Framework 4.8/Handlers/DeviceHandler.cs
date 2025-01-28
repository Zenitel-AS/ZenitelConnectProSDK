using ConnectPro.Models;
using ConnectPro.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Wamp.Client;
using static Wamp.Client.WampClient;
using Timer = System.Timers.Timer;

namespace ConnectPro.Handlers
{
    public class DeviceHandler
    {
        private Collections _collections;
        private Events _events;
        private Context _context;
        private WampClient _wamp;

        private Timer DeviceRetrievalTimer;

        public bool IsExecuting { get; set; } = false;

        public DeviceHandler(Collections collections,
                             Events events,
                             Context Context,
                             WampClient wamp)
        {
            _collections = collections;
            _events = events;
            _context = Context;
            _wamp = wamp;

            _wamp.OnWampDeviceRegistrationEvent += HandleDeviceRegistration;
            _events.OnDeviceRetrievalStart += HandleDeviceRetrievalStartEvent;
            _events.OnDeviceStateChange += HandleDeviceStateChange;
        }

        private void HandleDeviceRegistration(object sender, WampClient.wamp_device_registration_element ele)
        {
            Device newDevice = new Device(ele); //Convert wamp sdk element to Device class

            if (!_collections.RegisteredDevices.Any(x => x.dirno == ele.dirno))
            {
                _collections.RegisteredDevices.Add(newDevice);
                _context.Devices.Update(newDevice);
                _events.OnDeviceListChange?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                Device device = _collections.RegisteredDevices.FirstOrDefault(x => x.dirno == ele.dirno);
                if (device != null)
                {
                    _collections.RegisteredDevices.Remove(device);
                    _collections.RegisteredDevices.Add(newDevice);
                    _context.Devices.Update(newDevice);
                    _events.OnDeviceListChange?.Invoke(this, EventArgs.Empty);
                }
            }

        }
        private void HandleDeviceRetrievalStartEvent(object sender, EventArgs e)
        {
            SetDeviceRetrievalTimer();
        }
        private void HandleDeviceStateChange(object sender, object call_Element)
        {
            try
            {
                string to_dirno = "";
                string from_dirno = "";
                string to_dirno_current = "";
                string state = "";

                if (call_Element.GetType() == typeof(wamp_call_element))
                {
                    to_dirno = ((wamp_call_element)call_Element).to_dirno;
                    from_dirno = ((wamp_call_element)call_Element).from_dirno;
                    to_dirno_current = ((wamp_call_element)call_Element).to_dirno_current;
                    state = ((wamp_call_element)call_Element).state;
                }

                Device device_to_dirno = _collections.RegisteredDevices.Where(x => x.dirno == to_dirno).FirstOrDefault();
                Device device_to_dirno_current = _collections.RegisteredDevices.Where(x => x.dirno == to_dirno_current).FirstOrDefault();
                Device device_from_dirno = _collections.RegisteredDevices.Where(x => x.dirno == from_dirno).FirstOrDefault();

                if (device_to_dirno != null)
                    _collections.RegisteredDevices.Where(x => x.dirno == to_dirno).First().state = state == "ended" ? "reachable" : state;

                if (device_to_dirno_current != null)
                    _collections.RegisteredDevices.Where(x => x.dirno == to_dirno_current).First().state = state == "ended" ? "reachable" : state;

                if (device_from_dirno != null)
                    _collections.RegisteredDevices.Where(x => x.dirno == from_dirno).First().state = state == "ended" ? "reachable" : state;

                _events.OnDeviceListChange?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception exe)
            {
                _events.OnExceptionThrown?.Invoke(this, exe);
            }
        }

        public void SetDeviceRetrievalTimer()
        {
            if (DeviceRetrievalTimer != null)
            {
                DeviceRetrievalTimer.Stop();
                IsExecuting = false;
            }
            else
            {
                DeviceRetrievalTimer = new System.Timers.Timer(3000);
            }
            // Hook up the Elapsed event for the timer. 
            DeviceRetrievalTimer.Elapsed += RetrieveRegisteredDevices;
            DeviceRetrievalTimer.AutoReset = true;
            DeviceRetrievalTimer.Enabled = true;
        }
        public void RetrieveRegisteredDevices(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (!IsExecuting)
                {
                    IsExecuting = true;
                    _events.OnDebugChanged?.Invoke(this, (e.ToString(), e));


                    if (_wamp.IsConnected)
                    {
                        _collections.RegisteredDevices = GetRegisteredDevices().ToList();
                        //GetAllCalsAndQueues();
                        _events.OnQueuesAndCallsSync?.Invoke(this, new EventArgs());

                        if (_collections.RegisteredDevices != null)
                            DeviceRetrievalTimer?.Stop();

                        _events.OnDeviceListChange?.Invoke(this, new EventArgs());
                    }
                    IsExecuting = false;
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
        public IEnumerable<Device> GetRegisteredDevices()
        {
            return ObjectConverter.ConvertSdkDeviceElementList(_wamp.requestRegisteredDevices().ToList());
        }
    }
}
