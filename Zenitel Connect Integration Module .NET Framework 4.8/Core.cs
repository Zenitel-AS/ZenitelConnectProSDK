using ConnectPro.Handlers;
using System;
using Wamp.Client;

namespace ConnectPro
{
    public class Core
    {
        private Configuration _configuration = new Configuration();
        private Collections _collection = new Collections();
        private Events _events = new Events();
        private WampClient _wamp = new WampClient();

        public Configuration Configuration
        {
            get { return _configuration; }
            set { _configuration = value; }
        }
        public Collections Collection
        {
            get { return _collection; }
            set { _collection = value; }
        }
        public Events Events
        {
            get { return _events; }
            set { _events = value; }
        }
        public WampClient Wamp
        {
            get { return _wamp; }
            set { _wamp = value; }
        }


        public SystemMonitor SystemMonitor { get; set; }
        public DeviceHandler DeviceHandler { get; set; }
        public AudioEventHandler AudioEventHandler { get; set; }
        public DatabaseHandler DatabaseHandler { get; set; }
        public CallHandler CallHandler { get; set; }
        public ConnectionHandler ConnectionHandler { get; set; }
        public AccessControlHandler AccessControlHandler { get; set; }
        public BroadcastingHandler BroadcastingHandler { get; set; }

        public Debug.Log Log { get; set; }

        public Core()
        {
            try
            {
                this.Events.OnConnectionChanged += HandleConnectionChanged;
            }
            catch (Exception exe)
            {
                this.Events.OnExceptionThrown?.Invoke(this, exe);
            }
        }

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
            DeviceHandler = new DeviceHandler(ref _collection, ref _events, ref _wamp, this.Configuration.ServerAddr);
            AudioEventHandler = new AudioEventHandler(ref _events, ref _wamp, this.Configuration.ServerAddr);
            //DatabaseHandler = new DatabaseHandler(Collection, Events);
            CallHandler = new CallHandler(ref _collection, ref _events, ref _wamp, ref _configuration, this.Configuration.ServerAddr);
            ConnectionHandler = new ConnectionHandler(ref _collection, ref _events, ref _wamp, ref _configuration, this.Configuration.ServerAddr);
            AccessControlHandler = new AccessControlHandler(ref _events, ref _wamp, this.Configuration.ServerAddr);
            BroadcastingHandler = new BroadcastingHandler(ref _collection, ref _events, ref _wamp, this.Configuration.ServerAddr);
            Log = new Debug.Log(this.Collection, this.Events);
        }

        private void HandleConnectionChanged(object sender, bool isConnected)
        {
            try
            {
                switch (isConnected)
                {
                    case true:
                        Events.OnDeviceRetrievalStart?.Invoke(this, new EventArgs());
                        break;
                    case false:
                        break;
                }
            }
            catch (Exception exe)
            {
                this.Events.OnExceptionThrown?.Invoke(this, exe);
            }
        }
    }
}
