#pragma warning disable CS1591
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using WampSharp.Core.Listener;
using WampSharp.V2;
using WampSharp.V2.Client;
using WampSharp.V2.Core.Contracts;
using WampSharp.V2.Fluent;
using WampSharp.V2.Realm;


namespace Wamp.Client
{

    // no-WAMP login result serialization helper
    class json_login_result
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
    }

    /// <summary>
    /// This class implements a client connection using the WAMP protocol
    /// </summary>
    public partial class WampClient
    {
        /// <summary>This string defines the port number used for WAMP encrypted communication</summary>
        public const string WampEncryptedPort = "8086";

        /// <summary>This string defines the port number used for WAMP unencrypted communication</summary>
        public const string WampUnencryptedPort = "8087";

        /// <summary>This string defines the port number used for HTTPS communication</summary>
        public string HttpEncryptedPort = "443";

        /// <summary>This string defines the port number used for HTTP communication</summary>
        public string HttpUnencryptedPort = "80";


        //SYSTEM:

        /// <summary>Zenitel Link Path for accessing Registered Device Accounts</summary>
        //GET api/system/devices_accounts
        public const string GetWampRegisteredDevices = "com.zenitel.system.device_accounts";

        /// <summary>Zenitel Link Path for accessing Net Interfaces.</summary>
        //GET api/system/info/net_interfaces
        public const string GetWampInterfaceList = "com.zenitel.system.info.net_interfaces";

        /// <summary>Get list of configured device groups (group calls)</summary>
        /// GET api/groups
        public const string GetGroupsList = "com.zenitel.groups";

        /// <summary>Get a list of uploaded audio_messages</summary>
        /// GET api/system/audio_messages
        public const string GetAudioMessagesList = "com.zenitel.system.audio_messages";

        /// <summary>Get list of configured directory numbers</summary>
        /// GET api/directories
        public const string GetDirectoriesList = "com.zenitel.directory";

        //GET api/system/info/ntp
        //TBD public const string GetWampSystemInfoNtp     = "com.zenitel.system.info.ntp";

        //CALL HANDLING:

        /// <summary>Zenitel Link Path for setting up a call.</summary>
        //POST api/calls
        public const string PostWampCalls = "com.zenitel.calls.post";

        /// <summary>Zenitel Link Path for retrieving all calls.</summary>
        //GET api/calls
        public const string GetWampCalls = "com.zenitel.calls";

        /// <summary>Zenitel Link Path for deleting calls.</summary>
        //DELETE api/calls
        public const string DeleteWampCalls = "com.zenitel.calls.delete";

        /// <summary>Zenitel Link Path for retrieving the Call Legs.</summary>
        //GET api/calls_legs
        public const string GetCallLegs = "com.zenitel.call_legs";

        /// <summary>Zenitel Link Path for deleting a call using call ID.</summary>
        //DELETE api/calls/call{call_id}
        public const string DeleteWampCallsCallId = "com.zenitel.calls.call.delete";

        /// <summary>Zenitel Link Path for sending an action to a specific call.</summary>
        //POST api/calls/call{call_id} 
        public const string PostWampCallsCallId = "com.zenitel.calls.call.post";

        /// <summary>Get a list of all queued calls. Without arguments, all active queued calls are returned.
        /// Query paramters may be used to limit the selection. If multiple query parameters are provided,
        /// they are logically ANDed together which limits the selection further.</summary>
        //GET api/queues
        public const string GetWampQueues = "com.zenitel.call_queues";

        //DEVICE:

        /// <summary>Change a single General-Purpose Output (GPO), i.e. relay / gpio / e_relay controlled by a device.</summary>
        //POST api/devices/device/;{device_id}/gpos/gpo;{gpo_id}
        public const string PostWampDevicesGposGpoId = "com.zenitel.devices.device.gpos.gpo.post";

        /// <summary>Get all or some General-Purpose Output (GPO), i.e relay / gpio / e_relay controlled by a device.</summary>
        //GET api/devices/device/;{device_id}/gpos
        public const string GetWampDevicesGpos = "com.zenitel.devices.device.gpos";

        /// <summary>Get status of all or some General-Purpose Input (GPI) signals controlled by a device.</summary>
        //GET api/devices/device/;{device_id}/gpis
        public const string GetWampDevicesGpis = "com.zenitel.devices.device.gpis";

        //POST api/devices/device/;{device_id}/daks/dak;{dak_id}
        //TBD public const string PostWampDevicesDaksDakId = "com.zenitel.devices.device.daks.dak.post";

        //EVENTS:

        /// <summary>Subscribe to calls. Whenever a call is initiated, an event will be published on this channel.</summary>
        public const string TraceWampCalls = "com.zenitel.call";

        /// <summary>Subscribe to call leg events whenever a queued call state is changed</summary>
        public const string TraceWampCallLeg = "com.zenitel.call_leg";

        /// <summary>Subscribe on gpo/relay changes.</summary>
        public const string TraceWampDeviceDirnoGpo = "com.zenitel.device.{dirno}.gpo";
        //TBD public const string TraceWampDeviceDirnoDak = "com.zenitel.device.{dirno}.dak";

        /// <summary>Subscribe on gpi/gpio changes.</summary>
        public const string TraceWampDeviceDirnoGpi = "com.zenitel.device.{dirno}.gpi";

        /// <summary>Subscribe to device state changes.</summary>
        public const string TraceWampRegisteredDevices = "com.zenitel.system.device_account";

        /// <summary>Subscribe WAMP connection start event. The event is similar to
        /// https://wamp-proto.org/_static/gen/wamp_latest.html#x14-5-1-2-session-meta-events,
        /// except that the data is placed in 'arglist[0]', not in 'details'.</summary>
        public const string TraceWampSessionOnJoin = "com.zenitel.wamp.session.on_join";

        /// <summary>Subscribe WAMP connection close event.</summary>
        public const string TraceWampSessionOnLeave = "com.zenitel.wamp.session.on_leave";

        /// <summary>Dialing digit 6 in conversation will trigger an open door event.</summary>
        public const string TraceWampSystemOpenDoor = "com.zenitel.system.open_door";

        /// <summary>Subscribe to WAMP to publish est results for tone and button tests. This is only used for documenting wamp subscribe, HTTP GET to this URL does nothing.</summary>
        public const string TraceDeviceExtendedStatus = "com.zenitel.system.device.extended_status";

        /// <summary>Send dialling digit from station.</summary>
        public const string PostWampDevicesDeviceIdKey = "com.zenitel.devices.device.key.post";

        /// <summary>Send open door request from station.</summary>
        public const string PostWampOpenDoor = "com.zenitel.calls.call.open_door.post";

        /// <summary>Send request Zenitel Connect Software Version </summary>
        public const string GetPlatformVersion = "com.zenitel.system.platform.version";

        /// <summary>Audio Event detected: AED Server -> Zenitel Connect Pro. ZCP acts as brooker</summary>
        public const string TraceWampAudioEvents = "com.zenitel.public.audio_analytics";

        /// <summary>Audio Event Detector receives audio Stream</summary>
        public const string TraceWampAudioDataReceiving = "com.zenitel.public.audio_data_receiving";

        /// <summary>Audio Event Detector is alive and running</summary>
        public const string TraceWampAudioDetectorAlive = "com.zenitel.public.audio_detector_alive";

        /// <summary>Start Tone test for Dirno</summary>
        public const string PostWampToneTest = "com.zenitel.system.devices.test.tone.post";


        /// <summary>
        /// 
        /// </summary>
        // Provided services
        public const string Get_UCT_Time = "com.zenitel.system.get_uct_time";


        // Published events
        /// <summary>
        /// 
        /// </summary>
        public const string UCT_Time_event = "com.zenitel.system.uct_time";





        private Timer _reconnectTimer;


        /// <summary>Zenitel Connect Server IP Address.</summary>
        public string WampServerAddr = "169.254.1.5";


        /// <summary>Zenitel Connect Server IP Port Number.</summary>
        public string WampPort = WampEncryptedPort;


        /// <summary>Zenitel Connect WAMP URL.</summary>
        public string WampUrl => string.Format("wss://{0}:{1}/wamp", WampServerAddr, WampPort);


        /// <summary>Zenitel Link WAMP Realm.</summary>
        public string WampRealm = "zenitel";


        /// <summary>Zenitel Link Server Access User Name</summary>
        public string UserName = string.Empty;


        /// <summary>Zenitel Link Server Access Password</summary>
        public string Password = string.Empty;


        // authenticator is created when access token is retrieved
        TicketAuthenticator _wampAuthenticator;


        // created WAMP channel
        IWampChannel _wampChannel;


        // WAMP realm proxy - created 
        IWampRealmProxy _wampRealmProxy;


        /// <summary>WAMP connection established and session open for use.</summary>
        public bool IsConnected { get; private set; }


        /// <summary>Event Handler for WAMP connection change event.</summary>
        public event EventHandler<bool> OnConnectChanged;


        /// <summary>Event Handler for WAMP Error event.</summary>
        public event EventHandler<string> OnError;


        /// <summary>Event Handler for logging text.</summary>
        public event EventHandler<string> OnChildLogString;

        /// <summary>
        /// Defines the actions possible for a call
        /// </summary>
        public enum CallAction
        {
            /// <summary>
            /// Defines the call setup action
            /// </summary>
            setup,
            /// <summary>
            /// Defines the call answer action
            /// </summary>
            answer
        }


        private Timer RenewAccessTokenTimer = null;
        private bool RenewAccessTokenRequested = false;


        #region public methods

        /// <summary>
        /// Start of connection to server for obtaining access token.
        /// When token is obtained, tries open WAMP channel.
        /// HostAddr, WampRealm, UserName and Password must be set before Start.
        /// </summary>
        /***********************************************************************************************************************/
        public void Start()
        /***********************************************************************************************************************/
        {
            OnChildLogString?.Invoke(this, "WampConnection.Start().");
            StartReconnect();
        }


        /***********************************************************************************************************************/
        private void RenewAccessTokenTimer_Tick(object state)
        /***********************************************************************************************************************/
        {
            OnChildLogString?.Invoke(this, "RenewAccessTokenTimer timeout encountered.");

            if (RenewAccessTokenTimer != null)
            {
                //Delete timer
                RenewAccessTokenTimer.Dispose();
                RenewAccessTokenTimer = null;
                RenewAccessTokenRequested = false;
            }
 
            if (IsConnected)
            {
                RequestNewAcessToken();
            }
        }


        /// <summary>Stops opened connection or reconnecting</summary>
       /***********************************************************************************************************************/
        public void Stop()
        /***********************************************************************************************************************/
        {
            StopReconnect();
            ResetChannel();

            if (RenewAccessTokenTimer != null)
            {
                //Delete timer
                RenewAccessTokenTimer.Dispose();
                RenewAccessTokenTimer = null;
                RenewAccessTokenRequested = false;
            }
        }

        #endregion public methods


        #region internal connect


        /***********************************************************************************************************************/
        private bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
        /***********************************************************************************************************************/
        {
            //string txt = " ValidateRemoteCertificate. Certificate: " + certificate.ToString() +
            //             ". Chain: " + chain.ToString() + ". PolicyErrors: " + policyErrors.ToString();

            //OnChildLogString?.Invoke(this, txt);
            return true;
        }


        /***********************************************************************************************************************/
        private void StartReconnect()
        /***********************************************************************************************************************/
        {
            if (_reconnectTimer == null)
            {
                _reconnectTimer = new Timer((object s) =>
                {
                    try
                    {
                        bool useEncryption = WampPort.Equals(WampEncryptedPort);

                        // Authentication is HTTP / HTTPS 
                        string uri_str = ((useEncryption) ? ("https://" + WampServerAddr + ":" + HttpEncryptedPort) :
                                                            ("http://" + WampServerAddr)) +
                                                             "/api/auth/login";
                        Uri uri = new Uri(uri_str);

                        HttpWebRequest rq = (HttpWebRequest)WebRequest.Create(uri);
                        rq.Method = "POST";
                        rq.ContentType = "application/json";
                        rq.Accept = "application/json";
                        rq.Timeout = 5000;

                        string encoded = System.Convert.ToBase64String(
                            Encoding.GetEncoding("ISO-8859-1").GetBytes(UserName + ":" + Password));

                        rq.Headers.Add("Authorization", "Basic " + encoded);

                        ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);
                        #if NET48
                        // .NET Framework 4.8: Use TLS 1.3, 1.2, 1.1, and 1.0
                        ServicePointManager.SecurityProtocol =
                            SecurityProtocolType.Tls13 |
                            SecurityProtocolType.Tls12 |
                            SecurityProtocolType.Tls11 |
                            SecurityProtocolType.Tls;
                        #else
                        // Other frameworks: Use TLS 1.2, 1.1, and 1.0
                        ServicePointManager.SecurityProtocol =
                            SecurityProtocolType.Tls12 |
                            SecurityProtocolType.Tls11 |
                            SecurityProtocolType.Tls;
                        #endif

                        HttpWebResponse res = (HttpWebResponse)rq.GetResponse();
                        if (res.StatusCode == HttpStatusCode.OK)
                        {
                            var resstring = new StreamReader(res.GetResponseStream()).ReadToEnd();

                            json_login_result json_result = Newtonsoft.Json.JsonConvert.DeserializeObject<json_login_result>(resstring);

                            if (json_result == null)
                            {
                                SetConnectState(false, "null result");
                            }

                            else if (string.IsNullOrEmpty(json_result.access_token))
                            {
                                SetConnectState(false, "empty token");
                            }
                            else
                            {
                                OnChildLogString?.Invoke(this, "Access Token: " + json_result.access_token);
                                SetConnectState(true, null, json_result.access_token);
                            }
                        }
                        else
                        {
                            SetConnectState(false, "http request error: " + res.StatusCode + " " + res.StatusDescription);
                        }

                    }
                    catch (Exception ex)
                    {
                        OnChildLogString?.Invoke(this, "WampConnection.StartReconnect(). Exception: " + ex.ToString());
                        SetConnectState(false, "http request exception: " + ex.Message);
                    }
                });

                _reconnectTimer.Change(2000, 10000);
            }
            else
            {
                _reconnectTimer.Change(10000, 10000);
            }
        }

        /***********************************************************************************************************************/
        private void RequestNewAcessToken()
        /***********************************************************************************************************************/
        {
            try
            {
                bool useEncryption = WampPort.Equals(WampEncryptedPort);

                // Authentication is HTTP / HTTPS 
                string uri_str = ((useEncryption) ? ("https://" + WampServerAddr + ":" + HttpEncryptedPort) :
                                                    ("http://" + WampServerAddr)) +
                                                        "/api/auth/login";
                Uri uri = new Uri(uri_str);

                HttpWebRequest rq = (HttpWebRequest)WebRequest.Create(uri);
                rq.Method = "POST";
                rq.ContentType = "application/json";
                rq.Accept = "application/json";
                rq.Timeout = 5000;

                string encoded = System.Convert.ToBase64String(
                    Encoding.GetEncoding("ISO-8859-1").GetBytes(UserName + ":" + Password));

                rq.Headers.Add("Authorization", "Basic " + encoded);

                ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);
                #if NET48
                // .NET Framework 4.8: Use TLS 1.3, 1.2, 1.1, and 1.0
                ServicePointManager.SecurityProtocol =
                    SecurityProtocolType.Tls13 |
                    SecurityProtocolType.Tls12 |
                    SecurityProtocolType.Tls11 |
                    SecurityProtocolType.Tls;
                #else
                // Other frameworks: Use TLS 1.2, 1.1, and 1.0
                ServicePointManager.SecurityProtocol =
                    SecurityProtocolType.Tls12 |
                    SecurityProtocolType.Tls11 |
                    SecurityProtocolType.Tls;
                #endif
                HttpWebResponse res = (HttpWebResponse)rq.GetResponse();
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    var resstring = new StreamReader(res.GetResponseStream()).ReadToEnd();

                    json_login_result json_result = Newtonsoft.Json.JsonConvert.DeserializeObject<json_login_result>(resstring);

                    if (json_result != null)
                    {
                        if (! string.IsNullOrEmpty(json_result.access_token))
                        {
                            RenewAccessTokenRequested = true;
                            OnChildLogString?.Invoke(this, "Access Token: " + json_result.access_token);
                            SetConnectState(true, null, json_result.access_token);
                        }
                        else
                        {
                            SetConnectState(false, "http request error: " + res.StatusCode + " " + res.StatusDescription);
                        }
                    }
                    else
                    {
                        SetConnectState(false, "http request error: " + res.StatusCode + " " + res.StatusDescription);
                    }
                }
                else
                {
                    SetConnectState(false, "http request error: " + res.StatusCode + " " + res.StatusDescription);
                }

            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "WampConnection.StartReconnect(). Exception: " + ex.ToString());
                SetConnectState(false, "http request exception: " + ex.Message);
            }
        }


        /***********************************************************************************************************************/
        private void StopReconnect()
        /***********************************************************************************************************************/
        {
            if (_reconnectTimer != null)
            {
                _reconnectTimer.Dispose();
                _reconnectTimer = null;
            }
        }


        /***********************************************************************************************************************/
        private void ResetChannel()
        /***********************************************************************************************************************/
        {
            // close WAMP channel if it is
            if (_wampChannel != null)
            {
                try
                {
                    _wampChannel.Close();
                }
                catch (Exception ex)
                {
                    OnChildLogString?.Invoke(this, "Exception in ResetChannel(): " + ex.ToString());
                }
            }

            // reset proxy
            _wampRealmProxy = null;
        }


        /***********************************************************************************************************************/
        private void SetConnectState(bool connected, string error, string token = null)
        /***********************************************************************************************************************/
        {
             if (connected)
            {
                OnChildLogString?.Invoke(this, "WampConnection.SetConnectState. Connected: True.");

                StopReconnect();

                // create authenticator
                _wampAuthenticator = new TicketAuthenticator(UserName, token);

                // try to open channel
                OpenChannel();
            }
            else
            {
                OnChildLogString?.Invoke(this, "WampConnection.SetConnectState. Connected: False. Error: " + error);

                OnError?.Invoke(this, error);

                if (RenewAccessTokenTimer != null)
                {
                    //Delete timer
                    RenewAccessTokenTimer.Dispose();
                    RenewAccessTokenTimer = null;
                    RenewAccessTokenRequested = false;
                }

                // Start reconnect
                ResetChannel();
                Start();
            }
        }


        /***********************************************************************************************************************/
        private void OpenChannel()
        /***********************************************************************************************************************/
        {
            // create channel factory
            IWampChannelFactory factory = new WampChannelFactory();

            if (WampPort.Equals(WampEncryptedPort))
            {
                // create connect to realm, transport, serialization and authenticator
                var stx = factory
                    .ConnectToRealm(WampRealm)
                    .WebSocket4NetTransport(WampUrl)

                    .SetSecurityOptions(o =>
                    {
                        #if NET48
                        // .NET Framework 4.8: Include Tls13 and older protocols
                        o.EnabledSslProtocols = SslProtocols.Tls13 |
                                                 SslProtocols.Tls12 |
                                                 SslProtocols.Tls11 |
                                                 SslProtocols.Tls |
                                                 SslProtocols.Ssl3 |
                                                 SslProtocols.Ssl2;
                        #else
                        // Other frameworks: Exclude Tls13, fallback to older protocols
                        o.EnabledSslProtocols = SslProtocols.Tls12 |
                                                 SslProtocols.Tls11 |
                                                 SslProtocols.Tls |
                                                 SslProtocols.Ssl3 |
                                                 SslProtocols.Ssl2;
                        #endif

                        // Allow certificate chain errors
                        o.AllowCertificateChainErrors = true;
                        o.AllowNameMismatchCertificate = true;
                        o.AllowUnstrustedCertificate = true;
                    })


                    .JsonSerialization()
                    .Authenticator(_wampAuthenticator);

                // build channel
                _wampChannel = stx.Build();
            }
            else
            {
                //Build raw data connection
                var stx =
                factory.ConnectToRealm(WampRealm)
                       .RawSocketTransport(WampServerAddr, int.Parse(WampPort))
                       .JsonSerialization()
                       .Authenticator(_wampAuthenticator);
                _wampChannel = stx.Build();
            }

            // attach handlers to monitor
            _wampRealmProxy = _wampChannel.RealmProxy;
            _wampRealmProxy.Monitor.ConnectionEstablished += Monitor_ConnectionEstablished;
            _wampRealmProxy.Monitor.ConnectionError += Monitor_ConnectionError;
            _wampRealmProxy.Monitor.ConnectionBroken += Monitor_ConnectionBroken;

            try
            {
                _wampChannel.Open().Wait();
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception in OpenChannel(): " + ex.ToString());
            }
        }

        #endregion internal connect


        #region real proxy event handlers

        /***********************************************************************************************************************/
        private void Monitor_ConnectionEstablished(object sender, WampSessionCreatedEventArgs e)
        /***********************************************************************************************************************/
        {
            // notify connection is established
            IsConnected = true;

            if (RenewAccessTokenRequested)
            {
                RenewAccessTokenRequested = false;
            }
            else
            {
                OnConnectChanged?.Invoke(this, true);
            }

            // Start the timer for renewal of the access token.

            if (RenewAccessTokenTimer != null)
            {
                //Delete timer
                RenewAccessTokenTimer.Dispose();
                RenewAccessTokenTimer = null;
                RenewAccessTokenRequested = false;
            }

            // Timeout in ZCP is 30 minutes
            const Int32 minutes_29 = 29 * 60 * 1000; //ms

            RenewAccessTokenTimer = new Timer(RenewAccessTokenTimer_Tick, null, minutes_29, minutes_29);

            // For Test: RenewAccessTokenTimer = new Timer(RenewAccessTokenTimer_Tick, null, 10000, 10000);
        }


        /***********************************************************************************************************************/
        private void Monitor_ConnectionError(object sender, WampConnectionErrorEventArgs e)
        /***********************************************************************************************************************/
        {
            // notify connection establishing error
            IsConnected = false;
            OnConnectChanged?.Invoke(this, false);
        }


        /***********************************************************************************************************************/
        private void Monitor_ConnectionBroken(object sender, WampSessionCloseEventArgs e)
        /***********************************************************************************************************************/
        {
            // notify established connection is broken
            IsConnected = false;
            OnConnectChanged?.Invoke(this, false);
       
            // reconnect should be started
        }

        #endregion real proxy event handlers


        #region call functions from server



        /***********************************************************************************************************************/
        private object GetSystemDevicesRegistered()
        /***********************************************************************************************************************/
        {
            // get service
            var svc = _wampRealmProxy.Services.GetCalleeProxy<IConnectWampServices>();

            // try call function
            return svc.SystemDevicesRegistered();
        }



        /***********************************************************************************************************************/
        private object GetInterfaceList()
        /***********************************************************************************************************************/
        {
            // get service
            var svc = _wampRealmProxy.Services.GetCalleeProxy<IConnectWampServices>();

            // try call function
            return svc.InterfaceList();
        }


        /***********************************************************************************************************************/
        private object GET_calls(string dirNo, string callId, string state)
        /***********************************************************************************************************************/
        {
            try
            {
                // get service
                var svc = _wampRealmProxy.Services.GetCalleeProxy<IConnectWampServices>();

                // try call function
                return svc.GET_calls(dirNo, callId, state);

            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception in GET_calls: " + ex.ToString());
                return null;
            }
        }


        /***********************************************************************************************************************/
        private object GET_call_queue_legs(string fromDirNo, string toDirNo, string dirNo, string legId, string callId, string State, string legRole)
        /***********************************************************************************************************************/
        {
            try
            {
                // get service
                var svc = _wampRealmProxy.Services.GetCalleeProxy<IConnectWampServices>();

                // try call function
                return svc.GET_call_legs(fromDirNo, toDirNo, dirNo, legId, callId, State, legRole);
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception in GET_calls_queue: " + ex.ToString());
                return null;
            }
        }


        /***********************************************************************************************************************/
        private object GET_calls_queued(string queueDirNo)
        {
            try
            {
                if (_wampRealmProxy == null || !IsConnected)
                {
                    OnChildLogString?.Invoke(this, "GET_calls_queued: not connected (realm proxy not ready).");
                    return null;
                }

                var services = _wampRealmProxy.Services;
                if (services == null)
                {
                    OnChildLogString?.Invoke(this, "GET_calls_queued: realm proxy services not ready.");
                    return null;
                }

                var svc = services.GetCalleeProxy<IConnectWampServices>();
                if (svc == null)
                {
                    OnChildLogString?.Invoke(this, "GET_calls_queued: callee proxy unavailable.");
                    return null;
                }

                // IMPORTANT: omit optional param if empty
                return string.IsNullOrWhiteSpace(queueDirNo)
                    ? svc.GET_call_queues()
                    : svc.GET_call_queues(queueDirNo);
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception in GET_calls_queued: " + ex);
                return null;
            }
        }

        /***********************************************************************************************************************/
        private object GET_devices_gpos(string device_id, string id)
        {
            try
            {
                // The backend expects a payload with a 'dirno' key (not 'device_id').
                // Follow the same pattern as GET_devices_gpis: provide the payload as both
                // the single positional arg and as kwargs.
                var dirno = (device_id ?? string.Empty).Trim();

                var payload = new Dictionary<string, object>
                {
                    ["dirno"] = dirno
                };

                if (!string.IsNullOrWhiteSpace(id) && id != "*" && !id.Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    payload["id"] = id;
                }

                var rpcCallback = new RPCCallback();

                _wampRealmProxy.RpcCatalog.Invoke(
                    rpcCallback,
                    new CallOptions(),
                    WampClient.GetWampDevicesGpos,
                    new object[] { payload },     // args[0] = object
                    payload                        // kwargs = same object
                );

                // wait for response (same as your working method)
                bool cont = true;
                int loopCount = 0;
                const int sleepTime = 10;

                while (cont)
                {
                    Thread.Sleep(sleepTime);

                    if (rpcCallback.RespRecv) cont = false;
                    else if (++loopCount > 30) cont = false;
                }

                if (!rpcCallback.RespRecv)
                {
                    OnChildLogString?.Invoke(this, "GET_devices_gpos: No response from WAMP.");
                    return null;
                }

                if (!rpcCallback.CompletedSuccessfully)
                {
                    OnChildLogString?.Invoke(this, "GET_devices_gpos failed: " + rpcCallback.CompletionText);
                    return null;
                }

                // ✅ GET returns data
                return rpcCallback;
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception in GET_devices_gpos: " + ex);
                return null;
            }
        }



        private static string NormalizeDeviceId(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                return deviceId;

            // already in the expected form
            if (deviceId.StartsWith("device;", StringComparison.OrdinalIgnoreCase))
                return deviceId;

            // already has selector key, add the device; prefix
            if (deviceId.Contains("="))
                return "device;" + deviceId;

            // heuristic: MAC vs dirno
            if (deviceId.Contains(":") || deviceId.Contains("-"))
                return "device;mac_address=" + deviceId;

            return "device;dirno=" + deviceId;
        }



        /***********************************************************************************************************************/
        private object GET_devices_gpis(string device_id_or_dirno, string id)
        {
            try
            {
                var svc = _wampRealmProxy.Services.GetCalleeProxy<IConnectWampServices>();
                var response = svc.GET_devices_gpis(device_id_or_dirno, id);

                var dirno = (device_id_or_dirno ?? "").Trim();
                if (string.IsNullOrWhiteSpace(dirno))
                {
                    OnChildLogString?.Invoke(this, "GET_devices_gpis: missing dirno");
                    return null;
                }

                string outputId = (id ?? "").Trim();
                if (string.IsNullOrWhiteSpace(outputId) ||
                    outputId == "*" ||
                    outputId.Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    outputId = null;
                }

                var payload = new Dictionary<string, object>
                {
                    ["dirno"] = dirno
                };

                if (outputId != null)
                    payload["id"] = outputId;

                var rpcCallback = new RPCCallback();

                _wampRealmProxy.RpcCatalog.Invoke(
                    rpcCallback,
                    new WampSharp.V2.Core.Contracts.CallOptions(),
                    "com.zenitel.devices.device.gpis",
                    new object[] { payload },     // args[0] = object
                    payload                        // kwargs = same object
                );

                bool cont = true;
                int loopCount = 0;
                const int sleepTime = 10;

                while (cont)
                {
                    Thread.Sleep(sleepTime);

                    if (rpcCallback.RespRecv)
                    {
                        cont = false;
                    }
                    else
                    {
                        loopCount++;
                        if (loopCount > 30)
                            cont = false;
                    }
                }

                if (!rpcCallback.RespRecv)
                {
                    OnChildLogString?.Invoke(this, "GET_devices_gpis: No response from WAMP.");
                    return null;
                }

                if (!rpcCallback.CompletedSuccessfully)
                {
                    OnChildLogString?.Invoke(this, "GET_devices_gpis failed: " + rpcCallback.CompletionText);
                    return null;
                }

                return rpcCallback;
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception in GET_devices_gpis: " + ex);
                return null;
            }
        }


        /***********************************************************************************************************************/
        private object GET_PlatformVersion()
        /***********************************************************************************************************************/
        {
            try
            {
                // get service
                var svc = _wampRealmProxy.Services.GetCalleeProxy<IConnectWampServices>();

                // try call function
                return svc.GetPlatformVersion();
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception in GetPlatformVersion: " + ex.ToString());
                return null;
            }
        }

        /***********************************************************************************************************************/
        private object GET_groups(string dirno, bool verbose)
        /***********************************************************************************************************************/
        {
            try
            {
                // get service
                var svc = _wampRealmProxy.Services.GetCalleeProxy<IConnectWampServices>();

                // try call function
                return svc.GET_groups(dirno, verbose);

            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception in GET_devices_gpis: " + ex.ToString());
                return null;
            }
        }

        /***********************************************************************************************************************/
        private object GET_audio_messages()
        /***********************************************************************************************************************/
        {
            try
            {
                // get service
                var svc = _wampRealmProxy.Services.GetCalleeProxy<IConnectWampServices>();

                // try call function
                return svc.GET_audio_messages();

            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception in GET_devices_gpis: " + ex.ToString());
                return null;
            }
        }

        /***********************************************************************************************************************/
        private object GET_directories(string dirno)
        /***********************************************************************************************************************/
        {
            try
            {
                // get service
                var svc = _wampRealmProxy.Services.GetCalleeProxy<IConnectWampServices>();

                // try call function
                return svc.GET_directories(dirno);

            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception in GET_devices_gpis: " + ex.ToString());
                return null;
            }
        }
    }
    #endregion call functions from server
}

