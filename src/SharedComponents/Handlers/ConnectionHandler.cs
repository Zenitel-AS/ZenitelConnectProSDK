using System;
using System.Diagnostics;
using System.Timers;
using Wamp.Client;
using Timer = System.Timers.Timer;

namespace ConnectPro
{
    /// <summary>
    /// Handles the connection to the WAMP server, manages reconnection attempts,
    /// and listens for configuration changes.
    /// </summary>
    
    public class ConnectionHandler : IDisposable
    {
        #region Fields & Locks

        private Events _events;
        private WampClient _wamp;

        /// <summary>
        /// Maximum number of reconnection attempts.
        /// </summary>
        
        private int _maxReconnect = 50;

        /// <summary>
        /// Synchronization lock to prevent concurrent connection operations.
        /// </summary>
       
        private object _lockObject = new object();

        #endregion

        #region Properties
        
        /// <summary>
        /// Indicates whether the connection to the WAMP server is active.
        /// </summary>
        
        public bool IsConnected { get; private set; } = false;

        /// <summary>
        /// Indicates whether the system is attempting to reconnect.
        /// </summary>
        
        public bool IsReconnecting { get; private set; } = false;

        /// <summary>
        /// Stores the IP address of the parent device.
        /// </summary>
       
        public string ParentIpAddress { get; set; } = "";

        #endregion

        #region Constructor & Initialization

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionHandler"/> class.
        /// </summary>
        /// <param name="events">Reference to the events object handling connection-related events.</param>
        /// <param name="wamp">Reference to the WAMP client for server communication.</param>
        /// <param name="configuration">Reference to the configuration settings.</param>
        /// <param name="parentIpAddress">The IP address of the parent device.</param>
        
        public ConnectionHandler(ref Events events, ref WampClient wamp, ref Configuration configuration, string parentIpAddress)
        {
            _events = events;
            _wamp = wamp;

            // Apply configuration settings to the WAMP client.
            _wamp.WampServerAddr = configuration.ServerAddr;
            _wamp.WampPort = configuration.Port;
            _wamp.UserName = configuration.UserName;
            _wamp.Password = configuration.Password;
            ParentIpAddress = parentIpAddress;

            // Register event handlers.
            _wamp.OnConnectChanged += HandleConnectionChangeEvent_Internal;
            _events.OnConfigurationChanged += HandleConfigurationChangeEvent;
        }

        #endregion

        #region Event Handlers
        
        /// <summary>
        /// Handles changes in the WAMP connection state.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">Boolean indicating the connection status (true = connected, false = disconnected).</param>
        
        private void HandleConnectionChangeEvent_Internal(object sender, bool e)
        {
            try
            {
                this.IsConnected = _wamp.IsConnected;
                this.IsReconnecting = !_wamp.IsConnected;
                _events.OnConnectionChanged?.Invoke(this, this.IsConnected);

                switch (this.IsConnected)
                {
                    case true:
                        //Start WAMP tracing events
                        _wamp.TraceCallEvent();
                        _wamp.TraceCallLegEvent();
                        _wamp.TraceDeviceRegistrationEvent();
                        _wamp.TraceAudioEventDetection();
                        _wamp.TraceAudioDataReceiving();
                        _wamp.TraceAudioDetectorAlive();
                        _wamp.TraceOpenDoorEvent();

                        this.ResetRecconectionCounter();
                        break;
                    case false:
                        Recconect();
                        break;
                }
            }
            catch (Exception exe)
            {
                _events.OnExceptionThrown?.Invoke(this, exe);
            }
        }
        
        /// <summary>
        /// Handles configuration changes by updating the WAMP client settings.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="config">The updated configuration settings.</param>
        
        private void HandleConfigurationChangeEvent(object sender, ConnectPro.Configuration config)
        {
            this._wamp.WampServerAddr = config.ServerAddr;
            this._wamp.WampPort = config.Port;
            this._wamp.UserName = config.UserName;
            this._wamp.Password = config.Password;
        }

        #endregion

        #region Connection Management

        /// <summary>
        /// Opens a new connection to the WAMP server.
        /// </summary>
       
        public void OpenConnection()
        {
            lock (_lockObject)
            {
                _wamp.Start();
                IsReconnecting = true;
            }
        }
        
        /// <summary>
        /// Attempts to reconnect to the WAMP server, respecting the maximum reconnection limit.
        /// </summary>
        
        public void Recconect()
        {
            lock (_lockObject)
            {
                if (_maxReconnect > 0)
                {
                    _wamp.Stop();
                    _wamp.Start();
                    _maxReconnect--;
                }
            }
        }
        
        /// <summary>
        /// Resets the reconnection counter to allow further reconnect attempts.
        /// </summary>
        
        public void ResetRecconectionCounter()
        {
            _maxReconnect = 10;
        }

        #endregion

        #region IDisposable Implementation

        private bool _disposed = false;

        /// <summary>
        /// Disposes resources and unsubscribes from events.
        /// </summary>
        public void Dispose()
        {
            _wamp.Stop();
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
                if (_wamp != null)
                {
                    _wamp.OnConnectChanged -= HandleConnectionChangeEvent_Internal;
                }

                if (_events != null)
                {
                    _events.OnConfigurationChanged -= HandleConfigurationChangeEvent;
                }

                // If you have timers or additional managed resources added later, dispose them here.
            }

            _disposed = true;
        }

        #endregion

    }
}
