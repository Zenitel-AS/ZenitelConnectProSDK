using Microsoft.EntityFrameworkCore;
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
        private DbContext _context;
        private WampClient _wamp;

        public bool IsConnected { get; private set; } = false;
        public bool IsReconnecting { get; private set; } = false;

        private int _maxReconnect = 50;
        private object _lockObject = new object();

        public ConnectionHandler(Collections collections, Events events, DbContext Context, WampClient wamp, Configuration configuration)
        {
            _collections = collections;
            _events = events;
            _context = Context;
            _wamp = wamp;
            _wamp.WampServerAddr = configuration.ServerAddr;
            _wamp.WampPort = configuration.Port;
            _wamp.UserName = configuration.UserName;
            _wamp.Password = configuration.Password;

            _wamp.OnConnectChanged += HandleConnectionChangeEvent_Internal;
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
        public void OpenConnection()
        {
            _wamp.Start();
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
