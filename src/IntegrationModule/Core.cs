using ConnectPro.Handlers;
using ConnectPro.Models;
using System;
using System.Net.NetworkInformation;
using Wamp.Client;
using Zenitel.IntegrationModule.REST;

namespace ConnectPro
{
    /// <summary>
    /// Represents the core of the application, managing configurations, event handling, and system components.
    /// </summary>
    public class Core : IDisposable
    {
        #region Fields

        private Configuration _configuration = new Configuration();
        private Collections _collection = new Collections();
        private Events _events = new Events();
        private WampClient _wamp = new WampClient();
        private RestClient _rest = new RestClient();

        #endregion

        #region Properties

        private bool _isRuning = false;
        /// <summary>
        /// Indicates whether the core handlers are running.
        /// </summary>
        public bool IsRunning => _isRuning;

        /// <summary>
        /// Gets or sets the application configuration.
        /// </summary>
        public Configuration Configuration
        {
            get => _configuration;
            set
            {
                _configuration = value;
                SyncConfiguration();
            }
        }

        /// <summary>
        /// Gets or sets the collection of registered devices and related entities.
        /// </summary>
        public Collections Collection
        {
            get => _collection;
            set => _collection = value;
        }

        /// <summary>
        /// Gets or sets the event system for handling various application events.
        /// </summary>
        public Events Events
        {
            get => _events;
            set => _events = value;
        }

        /// <summary>
        /// Gets or sets the WAMP client responsible for communication.
        /// </summary>
        public WampClient Wamp
        {
            get => _wamp;
            set => _wamp = value;
        }

        /// <summary>
        /// Gets or sets the REST client for REST API communication.
        /// </summary>
        public RestClient Rest
        {
            get => _rest;
            set => _rest = value;
        }

        /// <summary>
        /// Gets or sets the system monitor responsible for tracking application status.
        /// </summary>
        public SystemMonitor SystemMonitor { get; set; }

        /// <summary>
        /// Gets or sets the device handler for managing registered devices.
        /// </summary>
        public DeviceHandler DeviceHandler { get; set; }

        /// <summary>
        /// Gets or sets the audio event handler for processing audio-related events.
        /// </summary>
        public AudioEventHandler AudioEventHandler { get; set; }

        /// <summary>
        /// Gets or sets the database handler for managing database interactions.
        /// </summary>
        public DatabaseHandler DatabaseHandler { get; set; }

        /// <summary>
        /// Gets or sets the call handler for processing and managing calls.
        /// </summary>
        public CallHandler CallHandler { get; set; }

        /// <summary>
        /// Gets or sets the connection handler for handling connections.
        /// </summary>
        public ConnectionHandler ConnectionHandler { get; set; }

        /// <summary>
        /// Gets or sets the access control handler for security management.
        /// </summary>
        public AccessControlHandler AccessControlHandler { get; set; }

        /// <summary>
        /// Gets or sets the broadcasting handler for managing announcements.
        /// </summary>
        public BroadcastingHandler BroadcastingHandler { get; set; }

        /// <summary>
        /// Gets or sets the call forwarding handler for managing call forwarding rules.
        /// </summary>
        public CallForwardingHandler CallForwardingHandler { get; set; }

        /// <summary>
        /// Gets or sets the log manager for handling debug logs.
        /// </summary>
        public Debug.Log Log { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Core"/> class and sets up event handling.
        /// </summary>
        public Core()
        {
            try
            {
                Events.OnConnectionChanged += HandleConnectionChanged;
                // Initialize configuration synchronization
                SyncConfiguration();
            }
            catch (Exception exe)
            {
                Events.OnExceptionThrown?.Invoke(this, exe);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Synchronizes the configuration between Core and both WampClient and RestClient.
        /// This ensures that changes to server address, username, or password are reflected in both clients.
        /// </summary>
        private void SyncConfiguration()
        {
            if (_configuration == null)
                return;

            // Synchronize WampClient
            if (_wamp != null)
            {
                _wamp.WampServerAddr = _configuration.ServerAddr;
                _wamp.UserName = _configuration.UserName;
                _wamp.Password = _configuration.Password;
            }

            // Synchronize RestClient
            if (_rest != null)
            {
                _rest.ServerAddress = _configuration.ServerAddr;
                _rest.UserName = _configuration.UserName;
                _rest.Password = _configuration.Password;
            }
        }

        /// <summary>
        /// Starts the core components of the application.
        /// </summary>
        public bool Start()
        {
            /*
             * Load order is important!
             * 
             * 1. System Monitor
             * 2. Device Handler
             * 3. CallHandler
             * 4. ConnectionHandler
             * 5. AccessControlHandler
             * 6. BroadcastingHandler
             * 7. Log
             * 
             */

            // Ensure RestClient is initialized and configured
            if (_rest == null)
            {
                _rest = new RestClient();
            }
            SyncConfiguration();

            SystemMonitor = new SystemMonitor(Events, Configuration, ref _wamp);
            // Use hybrid transport: REST for initial snapshots and WAMP for realtime events
            DeviceHandler = new DeviceHandler(ref _collection, ref _events, ref _wamp, new HybridGpioTransport(this, _wamp), Configuration.ServerAddr);
            AudioEventHandler = new AudioEventHandler(ref _events, ref _wamp, Configuration.ServerAddr);
            // DatabaseHandler = new DatabaseHandler(Collection, Events);
            CallHandler = new CallHandler(ref _collection, ref _events, ref _wamp, ref _configuration, Configuration.ServerAddr);
            ConnectionHandler = new ConnectionHandler(ref _events, ref _wamp, ref _rest, ref _configuration, Configuration.ServerAddr);
            AccessControlHandler = new AccessControlHandler(ref _events, ref _wamp, ref _configuration);
            BroadcastingHandler = new BroadcastingHandler(ref _collection, ref _events, ref _wamp, ref _rest, Configuration.ServerAddr);
            CallForwardingHandler = new CallForwardingHandler(ref _collection, ref _events, ref _wamp, ref _rest, Configuration.ServerAddr);
            Log = new Debug.Log(Collection, Events);

            _isRuning = true;
            return true;
        }

        /// <summary>
        /// Handles connection status changes.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="isConnected">Indicates whether the connection is active.</param>
        private void HandleConnectionChanged(object sender, bool isConnected)
        {
            try
            {
                if (isConnected)
                {
                    Events.OnDeviceRetrievalStart?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception exe)
            {
                Events.OnExceptionThrown?.Invoke(this, exe);
            }
        }
        #endregion

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
                // Unsubscribe from events
                if (_events != null)
                {
                    _events.OnConnectionChanged -= HandleConnectionChanged;
                }

                // Dispose managed resources
                SystemMonitor?.Dispose();
                DeviceHandler?.Dispose();
                AudioEventHandler?.Dispose();
                CallHandler?.Dispose();
                AccessControlHandler?.Dispose();
                BroadcastingHandler?.Dispose();
                CallForwardingHandler?.Dispose();
                Log?.Dispose();
                ConnectionHandler?.Dispose();

                // Ensure fields are cleared to release references
                SystemMonitor = null;
                DeviceHandler = null;
                AudioEventHandler = null;
                CallHandler = null;
                AccessControlHandler = null;
                BroadcastingHandler = null;
                CallForwardingHandler = null;
                Log = null;
                ConnectionHandler = null;
                _wamp = null;
                _rest = null;
            }

            _disposed = true;
        }

        #endregion

    }
}
