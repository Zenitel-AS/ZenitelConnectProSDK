using ConnectPro.Handlers;
using System;
using Wamp.Client;

namespace ConnectPro
{
    /// <summary>
    /// Represents the core of the application, managing configurations, event handling, and system components.
    /// </summary>
    public class Core
    {
        #region Fields

        private Configuration _configuration = new Configuration();
        private Collections _collection = new Collections();
        private Events _events = new Events();
        private WampClient _wamp = new WampClient();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the application configuration.
        /// </summary>
        public Configuration Configuration
        {
            get => _configuration;
            set => _configuration = value;
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
            }
            catch (Exception exe)
            {
                Events.OnExceptionThrown?.Invoke(this, exe);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts the core components of the application.
        /// </summary>
        public void Start()
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

            SystemMonitor = new SystemMonitor(Events, Configuration);
            DeviceHandler = new DeviceHandler(ref _collection, ref _events, ref _wamp, Configuration.ServerAddr);
            AudioEventHandler = new AudioEventHandler(ref _events, ref _wamp, Configuration.ServerAddr);
            // DatabaseHandler = new DatabaseHandler(Collection, Events);
            CallHandler = new CallHandler(ref _collection, ref _events, ref _wamp, ref _configuration, Configuration.ServerAddr);
            ConnectionHandler = new ConnectionHandler(ref _events, ref _wamp, ref _configuration, Configuration.ServerAddr);
            AccessControlHandler = new AccessControlHandler(ref _events, ref _wamp, Configuration.ServerAddr);
            BroadcastingHandler = new BroadcastingHandler(ref _collection, ref _events, ref _wamp, Configuration.ServerAddr);
            Log = new Debug.Log(Collection, Events);
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
    }
}
