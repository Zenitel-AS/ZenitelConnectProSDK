using ConnectPro.Models;
using ConnectPro.Models.GPIO;
using ConnectPro.Tools;
using SharedComponents.Models.GPIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    public class DeviceHandler : IDisposable
    {
        #region Fields & Locks

        private readonly Collections _collections;
        private readonly Events _events;
        private readonly WampClient _wamp;
        private readonly IGpioTransport _gpioTransport;

        /// <summary>
        /// Synchronization lock for device retrieval and registration operations.
        /// </summary>
        private readonly object _lockObj = new object();

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
                             IGpioTransport gpioTransport,
                             string parentIpAddress = "")
        {
            _collections = collections ?? throw new ArgumentNullException(nameof(collections));
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _wamp = wamp ?? throw new ArgumentNullException(nameof(wamp));
            ParentIpAddress = parentIpAddress ?? "";

            // GPIO transport (one per handler / one per WAMP client)
            _gpioTransport = gpioTransport;

            _wamp.OnWampDeviceRegistrationEvent += HandleDeviceRegistration;
            _wamp.OnWampDeviceExtendedStatusEvent += HandleDeviceExtendedStatus;
            _wamp.OnWampDeviceGPIStatusEventEx += HandleDeviceGPIOStatusEvent;
            _wamp.OnWampDeviceGPOStatusEventEx += HandleDeviceGPIOStatusEvent;

            _events.OnDeviceRetrievalStart += HandleDeviceRetrievalStartEvent;
            _events.OnDeviceStateChange += HandleDeviceStateChange;
            _events.OnDeviceRetrievalEnd += AttachGPIOListeners;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles device registration events from the WAMP client.
        /// </summary>
        private void HandleDeviceRegistration(object sender, WampClient.wamp_device_registration_element ele)
        {
            if (ele == null)
                return;

            // IMPORTANT: registration and retrieval both mutate the same list -> use the same lock.
            lock (_lockObj)
            {
                Device newDevice = new Device(ele, _gpioTransport); // Option A: Device owns its runtime GPIO

                if (_collections.RegisteredDevices == null)
                    _collections.RegisteredDevices = new List<Device>();

                if (!_collections.RegisteredDevices.Any(x => x.device_ip == ele.device_ip))
                {
                    _collections.RegisteredDevices.Add(newDevice);
                    _events.OnDeviceListChange?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    Device existing = _collections.RegisteredDevices.FirstOrDefault(x => x.device_ip == ele.device_ip);
                    if (existing != null)
                    {
                        // Dispose old GPIO runtime model before replacing.
                        DetachGpio(existing);

                        _collections.RegisteredDevices.Remove(existing);
                        _collections.RegisteredDevices.Add(newDevice);
                        _events.OnDeviceListChange?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        private void AttachGPIOListeners(object sender, EventArgs e)
        {
            //_events.OnChildLogEntry.Invoke(this, "Attaching GPIO Listeners");

            // Subscribe to global GPI/GPO events (single subscription for all devices)
            _wamp.TraceDeviceGPIStatusEvent();
            _wamp.TraceDeviceGPOStatusEvent();
        }

        /// <summary>
        /// Handles the start of the device retrieval process.
        /// </summary>
        private void HandleDeviceRetrievalStartEvent(object sender, EventArgs e)
        {
            Task.Run(async () => await RetrieveRegisteredDevices().ConfigureAwait(false));
        }

        /// <summary>
        /// Handles device state changes and updates the internal collections accordingly.
        /// </summary>
        private void HandleDeviceStateChange(object sender, CallElement callElement)
        {
            if (callElement == null)
                return;

            try
            {
                // Call-state updates mutate devices; you may also lock if you see concurrent list replacement issues.
                // Here we assume list replacement is guarded by _lockObj and this is mostly updating existing instances.
                Device device_to_dirno = _collections.RegisteredDevices?.FirstOrDefault(x => x.dirno == callElement.ToDirno);
                Device device_to_dirno_current = _collections.RegisteredDevices?.FirstOrDefault(x => x.dirno == callElement.ToDirnoCurrent);
                Device device_from_dirno = _collections.RegisteredDevices?.FirstOrDefault(x => x.dirno == callElement.FromDirno);

                if (device_to_dirno != null)
                    device_to_dirno.CallState =
                        callElement.CallState == Enums.CallState.ended ? Enums.CallState.reachable : callElement.CallState;

                if (device_to_dirno_current != null)
                    device_to_dirno_current.CallState =
                        callElement.CallState == Enums.CallState.ended ? Enums.CallState.reachable : callElement.CallState;

                if (device_from_dirno != null)
                    device_from_dirno.CallState =
                        callElement.CallState == Enums.CallState.ended ? Enums.CallState.reachable : callElement.CallState;

                _events.OnDeviceListChange?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception exe)
            {
                _events.OnExceptionThrown?.Invoke(this, exe);
            }
        }

        private void HandleDeviceExtendedStatus(object sender, WampClient.wamp_device_extended_status ele)
        {
            try
            {
                ExtendedStatus deviceStatus = new ExtendedStatus(ele);
                if (deviceStatus != null)
                    _events.OnDeviceTest?.Invoke(this, deviceStatus);
            }
            catch (Exception ex)
            {
                _events.OnExceptionThrown?.Invoke(this, ex);
            }
        }
        private void HandleDeviceGPIOStatusEvent(object sender, WampGpioEventArgs wampGpioEvent)
        {
            try
            {
                var device = _collections.RegisteredDevices.First(d => d.dirno == wampGpioEvent.Dirno);
                if (device == null)
                    throw new Exception($"HandleDeviceGPIOStatusEvent exception: No device found for dirno {wampGpioEvent.Dirno}");

                var gpio = device.Gpio.Inputs.FirstOrDefault(input => input.Id == wampGpioEvent.Element.id);
                if (gpio == null)
                    gpio = device.Gpio.Outputs.FirstOrDefault(output => output.Id == wampGpioEvent.Element.id);

                if (gpio != null)
                {
                    // Invoke GPIO Event here
                    _events.OnGpioEvent?.Invoke(this, wampGpioEvent);
                }
            }
            catch (Exception Ex)
            {
                _events.OnExceptionThrown?.Invoke(this, Ex);
            }

        }

        #endregion

        #region Device Retrieval & Management

        /// <summary>
        /// Retrieves the list of registered devices from the WAMP client and updates the collections.
        /// </summary>
        public async Task RetrieveRegisteredDevices()
        {
            try
            {
                // Guard: avoid concurrent retrievals
                lock (_lockObj)
                {
                    if (IsExecutingDeviceRetrieval)
                    {
                        _events.OnDeviceRetrievalEnd?.Invoke(this, EventArgs.Empty);
                        return;
                    }
                    IsExecutingDeviceRetrieval = true;
                }

                if (!_wamp.IsConnected)
                {
                    lock (_lockObj)
                    {
                        IsExecutingDeviceRetrieval = false;
                        _events.OnDeviceRetrievalEnd?.Invoke(this, EventArgs.Empty);
                    }
                    return;
                }

                // 1) Build devices + load GPIOs BEFORE touching RegisteredDevices
                var loadedDevices = await BuildDevicesWithGpioLoadedAsync().ConfigureAwait(false);

                // 2) Swap list under lock, after disposing old GPIO subscriptions
                lock (_lockObj)
                {
                    if (_collections.RegisteredDevices != null)
                    {
                        foreach (var old in _collections.RegisteredDevices)
                            DetachGpio(old);
                    }

                    _collections.RegisteredDevices = loadedDevices;
                    _events.OnQueuesAndCallsSync?.Invoke(this, EventArgs.Empty);

                    if (_collections.RegisteredDevices != null)
                        DeviceRetrievalTimer?.Stop();

                    _events.OnDeviceListChange?.Invoke(this, EventArgs.Empty);

                    IsExecutingDeviceRetrieval = false;
                    _events.OnDeviceRetrievalEnd?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                lock (_lockObj)
                {
                    IsExecutingDeviceRetrieval = false;
                    _events.OnDeviceRetrievalEnd?.Invoke(this, EventArgs.Empty);
                }

                _events.OnDebugChanged?.Invoke(this, (ex.Message, ex));
                _events.OnExceptionThrown?.Invoke(this, ex);
            }
        }


        /// <summary>
        /// Retrieves a list of registered devices from the WAMP client.
        /// </summary>
        public IEnumerable<Device> GetRegisteredDevices()
        {
            // Converter likely creates devices via the basic ctor. We attach runtime GPIO here as a safety net.
            var devices = ObjectConverter.ConvertSdkDeviceElementList(_wamp.requestRegisteredDevices().ToList());

            if (devices == null)
                return Enumerable.Empty<Device>();

            foreach (var d in devices)
                AttachGpio(d);

            return devices;
        }

        private async Task<List<Device>> BuildDevicesWithGpioLoadedAsync()
        {
            var devices = ObjectConverter
                .ConvertSdkDeviceElementList(_wamp.requestRegisteredDevices().ToList())
                ?.ToList()
                ?? new List<Device>();

            // Attach runtime GPIO first (starts subscribe + initial refresh)
            foreach (var d in devices)
                AttachGpio(d);

            // Await initial refresh for all devices (parallel)
            var initTasks = new List<Task>();

            foreach (var d in devices)
            {
                if (d == null || string.IsNullOrEmpty(d.dirno) || d.Gpio == null)
                    continue;

                initTasks.Add(AwaitGpioInitWithGuard(d));
            }

            // Optional: global timeout so retrieval isn't blocked forever
            // If you don't want timeout, remove WhenAny and just await Task.WhenAll(initTasks).
            var all = Task.WhenAll(initTasks);
            var timeout = Task.Delay(TimeSpan.FromSeconds(5)); // tune as you like

            var finished = await Task.WhenAny(all, timeout).ConfigureAwait(false);
            if (finished != all)
            {
                _events.OnDebugChanged?.Invoke(this, ("GPIO preload timeout (some devices did not finish initial snapshot in time).", null));
                // We proceed anyway: some devices will populate later via SafeInitialRefresh or live events.
            }

            return devices;
        }
        private async Task AwaitGpioInitWithGuard(Device d)
        {
            try
            {
                await d.Gpio.WhenInitializedAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _events.OnDebugChanged?.Invoke(this, ($"GPIO preload failed for {d.dirno}", ex));
            }
        }


        #endregion

        #region Device Control Methods

        public bool SimulateKeyPress(string dirno, string key, string edge)
        {
            wamp_response response = _wamp.PostDeviceIdKey(dirno, key, edge);
            if (response != null)
                return response.CompletionText == "PostKeyId sucessfully completed.";

            return false;
        }

        public bool InitiateToneTest(string dirno, string toneGroup)
        {
            wamp_response response = _wamp.ToneTest(dirno, toneGroup);
            if (response != null)
                return response.CompletionText == "ToneTest sucessfully completed.";

            return false;
        }

        private void AttachGpio(Device device)
        {
            if (device == null)
                return;

            // If device has no dirno, GPIO cannot be routed/subscribed (dirno is your routing key).
            if (string.IsNullOrEmpty(device.dirno))
                return;

            // Avoid re-attaching.
            if (device.Gpio != null)
                return;

            // Create runtime GPIO capability object.
            // Assumes DeviceGpio ctor: DeviceGpio(string dirno, IGpioTransport transport)
            device.Gpio = new DeviceGpio(device.dirno, _gpioTransport);
        }

        private void DetachGpio(Device device)
        {
            if (device == null)
                return;

            try
            {
                if (!string.IsNullOrEmpty(device.dirno))
                {
                    // Always ensure the transport drops callbacks/subscriptions for this device.
                    _gpioTransport.DisposeFor(device.dirno);
                }

                // If DeviceGpio is disposable, dispose it too (it may own timers/state).
                var disposable = device.Gpio as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }
            catch
            {
                // Do not throw from handlers; log if you have a hook.
            }
            finally
            {
                device.Gpio = null;
            }
        }

        #endregion

        #region IDisposable Implementation

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Unsubscribe from events
                if (_events != null)
                {
                    _events.OnDeviceRetrievalStart -= HandleDeviceRetrievalStartEvent;
                    _events.OnDeviceStateChange -= HandleDeviceStateChange;
                }

                // Detach GPIO for all known devices
                if (_collections?.RegisteredDevices != null)
                {
                    foreach (var d in _collections.RegisteredDevices)
                        DetachGpio(d);
                }

                // Dispose transport if possible
                var gpioDisp = _gpioTransport as IDisposable;
                if (gpioDisp != null)
                    gpioDisp.Dispose();

                if (_wamp != null)
                {
                    _wamp.OnWampDeviceRegistrationEvent -= HandleDeviceRegistration;
                    _wamp.OnWampDeviceExtendedStatusEvent -= HandleDeviceExtendedStatus;
                }

                // Dispose timer
                if (DeviceRetrievalTimer != null)
                {
                    DeviceRetrievalTimer.Stop();
                    DeviceRetrievalTimer.Dispose();
                    DeviceRetrievalTimer = null;
                }
            }

            _disposed = true;
        }

        #endregion
    }
}
