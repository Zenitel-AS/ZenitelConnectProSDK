using System;
using System.Diagnostics;
using System.Timers;
using Wamp.Client;
using Timer = System.Timers.Timer;

namespace ConnectPro
{
    public class ConnectionHandler
    {
        private Collections _collections;
        private Events _events;
        private WampClient _wamp;
        private Configuration _configuration;

        public bool IsConnected { get; private set; } = false;
        public bool IsReconnecting { get; private set; } = false;
        public string ParentIpAddress { get; set; } = "";

        private int _maxReconnect = 50;
        private object _lockObject = new object();
        
        public ConnectionHandler(ref Collections collections,ref Events events,ref WampClient wamp,ref Configuration configuration, string parentIpAddress)
        {
            _configuration = configuration;
            _collections = collections;
            _events = events;
            _wamp = wamp;
            _wamp.WampServerAddr = configuration.ServerAddr;
            _wamp.WampPort = configuration.Port;
            _wamp.UserName = configuration.UserName;
            _wamp.Password = configuration.Password;
            ParentIpAddress = parentIpAddress;

            _wamp.OnConnectChanged += HandleConnectionChangeEvent_Internal;
            _events.OnConfigurationChanged += HandleConfigurationChangeEvent;


        }
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
        private void HandleConfigurationChangeEvent(object sender, ConnectPro.Configuration config)
        {
            this._wamp.WampServerAddr = config.ServerAddr;
            this._wamp.WampPort = config.Port;
            this._wamp.UserName = config.UserName;
            this._wamp.Password = config.Password;
        }
        public void OpenConnection()
        {
            lock (_lockObject)
            {
                _wamp.Start();
                IsReconnecting = true;
            }
        }
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
        public void ResetRecconectionCounter()
        {
            _maxReconnect = 10;
        }
    }
}
