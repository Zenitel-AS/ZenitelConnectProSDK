#pragma warning disable CS1591
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        //CALL FORWARDING:

        /// <summary>Zenitel Link Path for retrieving call forwarding rules.</summary>
        //GET api/call_forwarding
        public const string GetCallForwarding = "com.zenitel.call_forwarding";

        /// <summary>Zenitel Link Path for adding or updating call forwarding rules.</summary>
        //POST api/call_forwarding
        public const string PostCallForwarding = "com.zenitel.call_forwarding.post";

        /// <summary>Zenitel Link Path for deleting call forwarding rules.</summary>
        //DELETE api/call_forwarding
        public const string DeleteCallForwarding = "com.zenitel.call_forwarding.delete";

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
        private readonly object _connectionGate = new object();
        private readonly SemaphoreSlim _channelTransitionGate = new SemaphoreSlim(1, 1);
        private bool _isStopping;
        private bool _certificateCallbackRegistered;
        private long _channelGeneration;
        private long _activeChannelGeneration;
        private ConnectionLifecycleState _connectionLifecycleState;


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

        private enum ConnectionLifecycleState
        {
            Disconnected,
            Connecting,
            Connected,
            Reconnecting,
            Stopping
        }

        private sealed class ChannelContext
        {
            public long Generation { get; set; }

            public IWampChannel Channel { get; set; }

            public IWampRealmProxy RealmProxy { get; set; }
        }


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
            LogLifecycleAction("WampConnection.Start() requested");

            lock (_connectionGate)
            {
                if (_isStopping)
                    return;
            }

            SetConnectionLifecycleState(ConnectionLifecycleState.Reconnecting, false);

            OnChildLogString?.Invoke(this, "WampConnection.Start().");
            StartReconnect();
        }


        /***********************************************************************************************************************/
        private void RenewAccessTokenTimer_Tick(object state)
        /***********************************************************************************************************************/
        {
            OnChildLogString?.Invoke(this, "RenewAccessTokenTimer timeout encountered.");

            StopRenewAccessTokenTimer();

            lock (_connectionGate)
            {
                if (_isStopping || !IsConnected)
                    return;
            }

            RequestNewAcessToken();
        }


        /// <summary>Stops opened connection or reconnecting</summary>
       /***********************************************************************************************************************/
        public void Stop()
        /***********************************************************************************************************************/
        {
            LogLifecycleAction("WampConnection.Stop() requested");

            lock (_connectionGate)
            {
                _isStopping = true;
                _connectionLifecycleState = ConnectionLifecycleState.Stopping;
            }

            StopReconnect();
            StopRenewAccessTokenTimer();
            ResetChannel();
            RemoveCertificateValidationCallback();
            ResetTraceSubscriptionsForReconnect();

            lock (_connectionGate)
            {
                IsConnected = false;
                RenewAccessTokenRequested = false;
                _connectionLifecycleState = ConnectionLifecycleState.Disconnected;
                _isStopping = false;
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
        private void StartRenewAccessTokenTimer()
        /***********************************************************************************************************************/
        {
            if (RenewAccessTokenTimer != null)
            {
                try
                {
                    RenewAccessTokenTimer.Dispose();
                }
                catch (Exception ex)
                {
                    OnChildLogString?.Invoke(this, "Exception disposing existing RenewAccessTokenTimer: " + ex);
                }
                finally
                {
                    RenewAccessTokenTimer = null;
                }
            }

            // ZCP access token timeout is 30 minutes.
            // Renew slightly before expiry.
            const int minutes_29 = 29 * 60 * 1000;

            RenewAccessTokenTimer = new Timer(
                RenewAccessTokenTimer_Tick,
                null,
                minutes_29,
                minutes_29);

            OnChildLogString?.Invoke(this, "RenewAccessTokenTimer started.");
        }
        /***********************************************************************************************************************/
        private void StopRenewAccessTokenTimer()
        /***********************************************************************************************************************/
        {
            if (RenewAccessTokenTimer != null)
            {
                RenewAccessTokenTimer.Dispose();
                RenewAccessTokenTimer = null;
            }

            RenewAccessTokenRequested = false;
        }

        /***********************************************************************************************************************/
        private void EnsureCertificateValidationCallback()
        /***********************************************************************************************************************/
        {
            if (_certificateCallbackRegistered)
                return;

            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            _certificateCallbackRegistered = true;
        }

        /***********************************************************************************************************************/
        private void RemoveCertificateValidationCallback()
        /***********************************************************************************************************************/
        {
            if (!_certificateCallbackRegistered)
                return;

            ServicePointManager.ServerCertificateValidationCallback -= ValidateRemoteCertificate;
            _certificateCallbackRegistered = false;
        }


        /***********************************************************************************************************************/
        private void StartReconnect()
        /***********************************************************************************************************************/
        {
            LogLifecycleAction("StartReconnect invoked");

            if (_reconnectTimer == null)
            {
                _reconnectTimer = new Timer((object s) =>
                {
                    try
                    {
                        lock (_connectionGate)
                        {
                            if (_isStopping)
                                return;
                        }

                        OnChildLogString?.Invoke(this, "Reconnect timer tick. " + GetConnectionDebugState());

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

                        EnsureCertificateValidationCallback();
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

                        using (HttpWebResponse res = (HttpWebResponse)rq.GetResponse())
                        {
                            if (res.StatusCode == HttpStatusCode.OK)
                            {
                                using (var reader = new StreamReader(res.GetResponseStream()))
                                {
                                    var resstring = reader.ReadToEnd();

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
                            }
                            else
                            {
                                SetConnectState(false, "http request error: " + res.StatusCode + " " + res.StatusDescription);
                            }
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
                LogLifecycleAction("RequestNewAcessToken invoked");

                lock (_connectionGate)
                {
                    if (_isStopping)
                        return;
                }

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

                EnsureCertificateValidationCallback();
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
                using (HttpWebResponse res = (HttpWebResponse)rq.GetResponse())
                {
                    if (res.StatusCode == HttpStatusCode.OK)
                    {
                        using (var reader = new StreamReader(res.GetResponseStream()))
                        {
                            var resstring = reader.ReadToEnd();

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
                    }
                    else
                    {
                        SetConnectState(false, "http request error: " + res.StatusCode + " " + res.StatusDescription);
                    }
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
        private void SetConnectionLifecycleState(ConnectionLifecycleState state, bool isConnected)
        /***********************************************************************************************************************/
        {
            lock (_connectionGate)
            {
                _connectionLifecycleState = state;
                IsConnected = isConnected;
            }
        }

        /***********************************************************************************************************************/
        private void SetConnectionLifecycleState(long generation, ConnectionLifecycleState state, bool isConnected)
        /***********************************************************************************************************************/
        {
            lock (_connectionGate)
            {
                if (generation != 0 && generation != _activeChannelGeneration)
                {
                    return;
                }

                _connectionLifecycleState = state;
                IsConnected = isConnected;
            }
        }

        /***********************************************************************************************************************/
        private long ReserveChannelGeneration()
        /***********************************************************************************************************************/
        {
            lock (_connectionGate)
            {
                return ++_channelGeneration;
            }
        }

        /***********************************************************************************************************************/
        private bool TryGetActiveRealmProxy(out IWampRealmProxy realmProxy, out long generation)
        /***********************************************************************************************************************/
        {
            lock (_connectionGate)
            {
                generation = _activeChannelGeneration;
                realmProxy = _wampRealmProxy;

                return realmProxy != null &&
                       IsConnected &&
                       _connectionLifecycleState == ConnectionLifecycleState.Connected;
            }
        }

        /***********************************************************************************************************************/
        private ChannelContext CaptureActiveChannelContext()
        /***********************************************************************************************************************/
        {
            lock (_connectionGate)
            {
                if (_wampChannel == null && _wampRealmProxy == null)
                {
                    return null;
                }

                return new ChannelContext
                {
                    Generation = _activeChannelGeneration,
                    Channel = _wampChannel,
                    RealmProxy = _wampRealmProxy
                };
            }
        }

        /***********************************************************************************************************************/
        private bool IsCurrentGeneration(long generation)
        /***********************************************************************************************************************/
        {
            lock (_connectionGate)
            {
                return generation != 0 && generation == _activeChannelGeneration;
            }
        }

        /***********************************************************************************************************************/
        private bool IsStoppingRequested()
        /***********************************************************************************************************************/
        {
            lock (_connectionGate)
            {
                return _isStopping;
            }
        }

        /***********************************************************************************************************************/
        private string GetConnectionDebugState()
        /***********************************************************************************************************************/
        {
            lock (_connectionGate)
            {
                return $"State={_connectionLifecycleState}, IsConnected={IsConnected}, ActiveGeneration={_activeChannelGeneration}, HasChannel={_wampChannel != null}, HasRealmProxy={_wampRealmProxy != null}, RenewRequested={RenewAccessTokenRequested}, IsStopping={_isStopping}";
            }
        }

        /***********************************************************************************************************************/
        private string GetConnectionDebugStateForRealm(IWampRealmProxy realmProxy)
        /***********************************************************************************************************************/
        {
            lock (_connectionGate)
            {
                var matchesActiveRealm = realmProxy != null && ReferenceEquals(realmProxy, _wampRealmProxy);

                return $"State={_connectionLifecycleState}, IsConnected={IsConnected}, ActiveGeneration={_activeChannelGeneration}, HasChannel={_wampChannel != null}, HasRealmProxy={_wampRealmProxy != null}, MatchesActiveRealm={matchesActiveRealm}, RenewRequested={RenewAccessTokenRequested}, IsStopping={_isStopping}";
            }
        }

        /***********************************************************************************************************************/
        private void LogLifecycleAction(string action, [CallerMemberName] string caller = null)
        /***********************************************************************************************************************/
        {
            OnChildLogString?.Invoke(this, $"{action}. Caller={caller}. {GetConnectionDebugState()}");
        }


        /***********************************************************************************************************************/
        private void ResetChannel()
        /***********************************************************************************************************************/
        {
            OnChildLogString?.Invoke(this, "ResetChannel BEGIN. " + GetConnectionDebugState());

            _channelTransitionGate.Wait();

            try
            {
                var channelContext = CaptureActiveChannelContext();

                OnChildLogString?.Invoke(this,
                    $"ResetChannel captured context Generation={channelContext?.Generation ?? 0}, HasChannel={channelContext?.Channel != null}, HasRealmProxy={channelContext?.RealmProxy != null}.");

                lock (_connectionGate)
                {
                    _wampChannel = null;
                    _wampRealmProxy = null;
                    _activeChannelGeneration = 0;
                    _connectionLifecycleState = _isStopping
                        ? ConnectionLifecycleState.Stopping
                        : ConnectionLifecycleState.Disconnected;
                    IsConnected = false;
                }

                DisposeChannelContext(channelContext, "ResetChannel");

                OnChildLogString?.Invoke(this, "ResetChannel END. " + GetConnectionDebugState());
            }
            finally
            {
                _channelTransitionGate.Release();
            }
        }

        /***********************************************************************************************************************/
        private void SetConnectState(bool connected, string error, string token = null)
        /***********************************************************************************************************************/
        {
            OnChildLogString?.Invoke(this,
                $"SetConnectState invoked. Connected={connected}, Error='{error ?? "<null>"}', HasToken={!string.IsNullOrEmpty(token)}. {GetConnectionDebugState()}");

            lock (_connectionGate)
            {
                if (_isStopping)
                    return;
            }

            if (connected)
            {
                OnChildLogString?.Invoke(this, "WampConnection.SetConnectState. Connected: True.");
                OnChildLogString?.Invoke(this, "SetConnectState(TRUE) before transition. " + GetConnectionDebugState());

                StopReconnect();

                SetConnectionLifecycleState(ConnectionLifecycleState.Connecting, false);

                _wampAuthenticator = new TicketAuthenticator(UserName, token);

                OpenChannel();
            }
            else
            {
                OnChildLogString?.Invoke(this, "WampConnection.SetConnectState. Connected: False. Error: " + error);
                OnChildLogString?.Invoke(this, "SetConnectState(FALSE) before reset. " + GetConnectionDebugState());

                OnError?.Invoke(this, error);

                StopRenewAccessTokenTimer();

                SetConnectionLifecycleState(ConnectionLifecycleState.Disconnected, false);

                ResetChannel();
                Start();
            }
        }


        /***********************************************************************************************************************/
        private void OpenChannel()
        /***********************************************************************************************************************/
        {
            OnChildLogString?.Invoke(this, "OpenChannel BEGIN. " + GetConnectionDebugState());

            _channelTransitionGate.Wait();

            ChannelContext previousChannelContext = null;
            ChannelContext newChannelContext = null;

            try
            {
                var generation = ReserveChannelGeneration();
                OnChildLogString?.Invoke(this, $"OpenChannel reserved Generation={generation}. {GetConnectionDebugState()}");

                newChannelContext = BuildChannelContext(generation);
                AttachMonitorEvents(newChannelContext);

                OnChildLogString?.Invoke(this,
                    $"OpenChannel built channel Generation={generation}, HasChannel={newChannelContext.Channel != null}, HasRealmProxy={newChannelContext.RealmProxy != null}.");

                lock (_connectionGate)
                {
                    previousChannelContext = new ChannelContext
                    {
                        Generation = _activeChannelGeneration,
                        Channel = _wampChannel,
                        RealmProxy = _wampRealmProxy
                    };

                    _wampChannel = newChannelContext.Channel;
                    _wampRealmProxy = newChannelContext.RealmProxy;
                    _activeChannelGeneration = newChannelContext.Generation;
                    _connectionLifecycleState = ConnectionLifecycleState.Connecting;
                    IsConnected = false;
                }

                OnChildLogString?.Invoke(this,
                    $"OpenChannel published Generation={newChannelContext.Generation}. PreviousGeneration={previousChannelContext?.Generation ?? 0}. {GetConnectionDebugState()}");

                newChannelContext.Channel.Open().Wait();

                OnChildLogString?.Invoke(this,
                    $"OpenChannel OPEN completed Generation={newChannelContext.Generation}. {GetConnectionDebugState()}");

                DisposeChannelContext(previousChannelContext, "OpenChannel previous channel swap");
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception in OpenChannel(): " + ex.ToString());

                if (newChannelContext != null)
                {
                    lock (_connectionGate)
                    {
                        if (_activeChannelGeneration == newChannelContext.Generation)
                        {
                            _wampChannel = previousChannelContext?.Channel;
                            _wampRealmProxy = previousChannelContext?.RealmProxy;
                            _activeChannelGeneration = previousChannelContext?.Generation ?? 0;
                            _connectionLifecycleState = previousChannelContext != null && previousChannelContext.Channel != null
                                ? ConnectionLifecycleState.Reconnecting
                                : ConnectionLifecycleState.Disconnected;
                            IsConnected = false;
                        }
                    }

                    DisposeChannelContext(newChannelContext, "OpenChannel failed new channel");
                }

                OnChildLogString?.Invoke(this, "OpenChannel FAILURE state. " + GetConnectionDebugState());
            }
            finally
            {
                _channelTransitionGate.Release();
            }
        }

        /***********************************************************************************************************************/
        private ChannelContext BuildChannelContext(long generation)
        /***********************************************************************************************************************/
        {
            IWampChannelFactory factory = new WampChannelFactory();
            IWampChannel channel;

            OnChildLogString?.Invoke(this,
                $"BuildChannelContext Generation={generation}, Transport={(WampPort.Equals(WampEncryptedPort) ? "WebSocket4NetTransport" : "RawSocketTransport")}, Url={WampUrl}, Realm={WampRealm}.");

            if (WampPort.Equals(WampEncryptedPort))
            {
                var stx = factory
                    .ConnectToRealm(WampRealm)
                    .WebSocket4NetTransport(WampUrl)
                    .SetSecurityOptions(o =>
                    {
                        #if NET48
                        o.EnabledSslProtocols = SslProtocols.Tls13 |
                                                 SslProtocols.Tls12 |
                                                 SslProtocols.Tls11 |
                                                 SslProtocols.Tls |
                                                 SslProtocols.Ssl3 |
                                                 SslProtocols.Ssl2;
                        #else
                        o.EnabledSslProtocols = SslProtocols.Tls12 |
                                                 SslProtocols.Tls11 |
                                                 SslProtocols.Tls |
                                                 SslProtocols.Ssl3 |
                                                 SslProtocols.Ssl2;
                        #endif

                        o.AllowCertificateChainErrors = true;
                        o.AllowNameMismatchCertificate = true;
                        o.AllowUnstrustedCertificate = true;
                    })
                    .JsonSerialization()
                    .Authenticator(_wampAuthenticator);

                channel = stx.Build();
            }
            else
            {
                var stx = factory
                    .ConnectToRealm(WampRealm)
                    .RawSocketTransport(WampServerAddr, int.Parse(WampPort))
                    .JsonSerialization()
                    .Authenticator(_wampAuthenticator);

                channel = stx.Build();
            }

            return new ChannelContext
            {
                Generation = generation,
                Channel = channel,
                RealmProxy = channel.RealmProxy
            };
        }

        /***********************************************************************************************************************/
        private void AttachMonitorEvents(ChannelContext channelContext)
        /***********************************************************************************************************************/
        {
            if (channelContext?.RealmProxy?.Monitor == null)
            {
                return;
            }

            channelContext.RealmProxy.Monitor.ConnectionEstablished += Monitor_ConnectionEstablished;
            channelContext.RealmProxy.Monitor.ConnectionError += Monitor_ConnectionError;
            channelContext.RealmProxy.Monitor.ConnectionBroken += Monitor_ConnectionBroken;
        }

        /***********************************************************************************************************************/
        private void DetachMonitorEvents(ChannelContext channelContext)
        /***********************************************************************************************************************/
        {
            if (channelContext?.RealmProxy?.Monitor == null)
            {
                return;
            }

            channelContext.RealmProxy.Monitor.ConnectionEstablished -= Monitor_ConnectionEstablished;
            channelContext.RealmProxy.Monitor.ConnectionError -= Monitor_ConnectionError;
            channelContext.RealmProxy.Monitor.ConnectionBroken -= Monitor_ConnectionBroken;
        }

        /***********************************************************************************************************************/
        private void DisposeChannelContext(ChannelContext channelContext, string source)
        /***********************************************************************************************************************/
        {
            if (channelContext == null)
            {
                return;
            }

            DetachMonitorEvents(channelContext);

            if (channelContext.Channel == null)
            {
                return;
            }

            try
            {
                OnChildLogString?.Invoke(this,
                    $"DisposeChannelContext Source={source}, Generation={channelContext.Generation}, HasChannel={channelContext.Channel != null}, HasRealmProxy={channelContext.RealmProxy != null}.");

                channelContext.Channel.Close();
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, $"Exception disposing channel in {source}: {ex}");
            }
        }

        /***********************************************************************************************************************/
        private bool TryGetActiveGenerationForMonitor(object sender, out long generation)
        /***********************************************************************************************************************/
        {
            lock (_connectionGate)
            {
                generation = _activeChannelGeneration;

                return generation != 0 &&
                       _wampRealmProxy?.Monitor != null &&
                       ReferenceEquals(sender, _wampRealmProxy.Monitor);
            }
        }

        /***********************************************************************************************************************/
        private bool TryGetActiveServices(out IWampRealmProxy realmProxy, out IConnectWampServices serviceProxy, out long generation, string operationName)
        /***********************************************************************************************************************/
        {
            serviceProxy = null;

            if (!TryGetActiveRealmProxy(out realmProxy, out generation))
            {
                OnChildLogString?.Invoke(this, operationName + ": not connected. " + GetConnectionDebugState());
                return false;
            }

            var services = realmProxy.Services;
            if (services == null)
            {
                OnChildLogString?.Invoke(this, operationName + $": realm proxy services not ready. Generation={generation}. " + GetConnectionDebugState());
                return false;
            }

            serviceProxy = services.GetCalleeProxy<IConnectWampServices>();
            if (serviceProxy == null)
            {
                OnChildLogString?.Invoke(this, operationName + $": callee proxy unavailable. Generation={generation}. " + GetConnectionDebugState());
                return false;
            }

            OnChildLogString?.Invoke(this,
                operationName + $": active service proxy acquired. Generation={generation}, ServicesHash={services.GetHashCode()}, ServiceProxyHash={serviceProxy.GetHashCode()}. " + GetConnectionDebugState());

            return true;
        }

        /***********************************************************************************************************************/
        private bool TryGetActiveRpcCatalog(out IWampRealmProxy realmProxy, out long generation, string operationName)
        /***********************************************************************************************************************/
        {
            if (!TryGetActiveRealmProxy(out realmProxy, out generation))
            {
                OnChildLogString?.Invoke(this, operationName + ": not connected. " + GetConnectionDebugState());
                return false;
            }

            if (realmProxy.RpcCatalog == null)
            {
                OnChildLogString?.Invoke(this, operationName + $": RPC catalog not ready. Generation={generation}. " + GetConnectionDebugState());
                return false;
            }

            OnChildLogString?.Invoke(this,
                operationName + $": active RPC catalog acquired. Generation={generation}, RpcCatalogHash={realmProxy.RpcCatalog.GetHashCode()}. " + GetConnectionDebugState());

            return true;
        }

        /***********************************************************************************************************************/
        private void HandleExpectedRpcFailure(string operationName, Exception ex)
        /***********************************************************************************************************************/
        {
            if (ex is WampConnectionBrokenException || IsExpectedTransportAbort(ex))
            {
                OnChildLogString?.Invoke(this, operationName + ": transport closed during RPC: " + ex.Message + ". " + GetConnectionDebugState());
                MarkWampConnectionBroken(operationName);
                return;
            }

            OnChildLogString?.Invoke(this, operationName + ": " + ex + ". " + GetConnectionDebugState());
        }

        #endregion internal connect


        #region real proxy event handlers

        /***********************************************************************************************************************/
        private async void Monitor_ConnectionEstablished(object sender, WampSessionCreatedEventArgs e)
        {
            try
            {
                if (!TryGetActiveGenerationForMonitor(sender, out var generation))
                {
                    OnChildLogString?.Invoke(this, "Ignoring stale WAMP connection established callback.");
                    return;
                }

                OnChildLogString?.Invoke(this, $"WAMP connection established. Generation={generation}. " + GetConnectionDebugState());

                SetConnectionLifecycleState(generation, ConnectionLifecycleState.Connected, true);

                await RegisterCalleeServices().ConfigureAwait(false);

                if (RenewAccessTokenRequested)
                {
                    OnChildLogString?.Invoke(this, $"Monitor_ConnectionEstablished: renew flow detected for Generation={generation}.");
                    RenewAccessTokenRequested = false;
                }
                else
                {
                    OnChildLogString?.Invoke(this, $"Monitor_ConnectionEstablished: raising OnConnectChanged(true) for Generation={generation}.");
                    OnConnectChanged?.Invoke(this, true);
                }

                StartRenewAccessTokenTimer();
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this,
                    "Exception in Monitor_ConnectionEstablished: " + ex);

                MarkWampConnectionBroken("Monitor_ConnectionEstablished");
            }
        }


        /***********************************************************************************************************************/
        private void Monitor_ConnectionError(object sender, WampConnectionErrorEventArgs e)
        /***********************************************************************************************************************/
        {
            if (!TryGetActiveGenerationForMonitor(sender, out var generation))
            {
                OnChildLogString?.Invoke(this, "Ignoring stale WAMP connection error callback.");
                return;
            }

            OnChildLogString?.Invoke(this, $"WAMP connection error: Generation={generation}, Exception={e.Exception}. " + GetConnectionDebugState());

            SetConnectionLifecycleState(generation, ConnectionLifecycleState.Reconnecting, false);

            Task.Run(async () =>
            {
                await RegisterCalleeServicesDisposeAsync().ConfigureAwait(false);
                StartReconnect();
            });

            OnConnectChanged?.Invoke(this, false);
        }


        /***********************************************************************************************************************/
        private void Monitor_ConnectionBroken(object sender, WampSessionCloseEventArgs e)
        {
            if (!TryGetActiveGenerationForMonitor(sender, out var generation))
            {
                OnChildLogString?.Invoke(this, "Ignoring stale WAMP connection broken callback.");
                return;
            }

            OnChildLogString?.Invoke(this, $"WAMP connection broken. Generation={generation}, CloseType={e.CloseType}. " + GetConnectionDebugState());

            SetConnectionLifecycleState(generation, ConnectionLifecycleState.Reconnecting, false);

            Task.Run(async () =>
            {
                await RegisterCalleeServicesDisposeAsync().ConfigureAwait(false);
                StartReconnect();
            });

            OnConnectChanged?.Invoke(this, false);
        }

        private void MarkWampConnectionBroken(string source)
        {
            try
            {
                var channelContext = CaptureActiveChannelContext();
                var generation = channelContext?.Generation ?? 0;

                if (generation == 0 || !IsCurrentGeneration(generation))
                {
                    OnChildLogString?.Invoke(this,
                        "Ignoring broken-state transition from stale channel in " + source + ".");
                    return;
                }

                OnChildLogString?.Invoke(this,
                    $"WAMP connection marked broken by {source}. Generation={generation}. " + GetConnectionDebugState());

                SetConnectionLifecycleState(generation, ConnectionLifecycleState.Reconnecting, false);

                Task.Run(async () =>
                {
                    await RegisterCalleeServicesDisposeAsync().ConfigureAwait(false);
                    StartReconnect();
                });

                OnConnectChanged?.Invoke(this, false);
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this,
                    "Exception in MarkWampConnectionBroken: " + ex);
            }
        }

        #endregion real proxy event handlers


        #region call functions from server



        /***********************************************************************************************************************/
        private object GetSystemDevicesRegistered()
        /***********************************************************************************************************************/
        {
            try
            {
                if (!TryGetActiveServices(out _, out var svc, out _, "GetSystemDevicesRegistered"))
                {
                    return null;
                }

                return svc.SystemDevicesRegistered();
            }
            catch (WampConnectionBrokenException ex)
            {
                HandleExpectedRpcFailure("GetSystemDevicesRegistered", ex);
                return null;
            }
            catch (Exception ex)
            {
                HandleExpectedRpcFailure("GetSystemDevicesRegistered", ex);
                return null;
            }
        }



        /***********************************************************************************************************************/
        private object GetInterfaceList()
        /***********************************************************************************************************************/
        {
            try
            {
                if (!TryGetActiveServices(out _, out var svc, out _, "GetInterfaceList"))
                {
                    return null;
                }

                return svc.InterfaceList();
            }
            catch (Exception ex)
            {
                HandleExpectedRpcFailure("GetInterfaceList", ex);
                return null;
            }
        }


        /***********************************************************************************************************************/
        private object GET_calls(string dirNo, string callId, string state)
        /***********************************************************************************************************************/
        {
            try
            {
                if (!TryGetActiveServices(out _, out var svc, out _, "GET_calls"))
                {
                    return null;
                }

                return svc.GET_calls(dirNo, callId, state);

            }
            catch (Exception ex)
            {
                HandleExpectedRpcFailure("GET_calls", ex);
                return null;
            }
        }


        /***********************************************************************************************************************/
        private object GET_call_queue_legs(string fromDirNo, string toDirNo, string dirNo, string legId, string callId, string State, string legRole)
        /***********************************************************************************************************************/
        {
            try
            {
                if (!TryGetActiveServices(out _, out var svc, out _, "GET_call_queue_legs"))
                {
                    return null;
                }

                return svc.GET_call_legs(fromDirNo, toDirNo, dirNo, legId, callId, State, legRole);
            }
            catch (Exception ex)
            {
                HandleExpectedRpcFailure("GET_call_queue_legs", ex);
                return null;
            }
        }


        /***********************************************************************************************************************/
        private object GET_calls_queued(string queueDirNo)
        {
            var callId = Guid.NewGuid().ToString("N");

            try
            {
                OnChildLogString?.Invoke(this,
                    $"GET_calls_queued START CallId={callId}, QueueDirNo='{queueDirNo ?? "<null>"}'");

                if (!TryGetActiveServices(out _, out var svc, out _, "GET_calls_queued"))
                {
                    OnChildLogString?.Invoke(this,
                        $"GET_calls_queued ABORT unable to acquire active service proxy CallId={callId}");
                    return null;
                }

                OnChildLogString?.Invoke(this,
                    $"GET_calls_queued PROXY READY CallId={callId}");

                var task = System.Threading.Tasks.Task.Run(() =>
                {
                    OnChildLogString?.Invoke(this,
                        $"GET_calls_queued ENTER RPC CallId={callId}, Thread={System.Threading.Thread.CurrentThread.ManagedThreadId}");

                    object result;

                    // IMPORTANT: omit optional param if empty
                    if (string.IsNullOrWhiteSpace(queueDirNo))
                    {
                        OnChildLogString?.Invoke(this,
                            $"GET_calls_queued CALL no-arg CallId={callId}. {GetConnectionDebugState()}");

                        result = svc.GET_call_queues();
                    }
                    else
                    {
                        OnChildLogString?.Invoke(this,
                            $"GET_calls_queued CALL one-arg CallId={callId}, queue_dirno='{queueDirNo}'. {GetConnectionDebugState()}");

                        result = svc.GET_call_queues(queueDirNo);
                    }

                    OnChildLogString?.Invoke(this,
                        $"GET_calls_queued EXIT RPC CallId={callId}, ResultNull={result == null}");

                    return result;
                });

                if (!task.Wait(TimeSpan.FromSeconds(10)))
                {
                    ObserveTimedOutRpcTask(task, callId, queueDirNo);

                    OnChildLogString?.Invoke(this,
                        $"GET_calls_queued TIMEOUT CallId={callId}, QueueDirNo='{queueDirNo ?? "<null>"}'");

                    return null;
                }

                var finalResult = task.GetAwaiter().GetResult();

                OnChildLogString?.Invoke(this,
                    $"GET_calls_queued END CallId={callId}, ResultNull={finalResult == null}");

                return finalResult;
            }
            catch (WampConnectionBrokenException ex)
            {
                HandleExpectedRpcFailure("GET_calls_queued", ex);
                return null;
            }
            catch (Exception ex)
            {
                HandleExpectedRpcFailure("GET_calls_queued", ex);
                return null;
            }
        }

        /***********************************************************************************************************************/
        private void ObserveTimedOutRpcTask(System.Threading.Tasks.Task<object> task, string callId, string queueDirNo)
        /***********************************************************************************************************************/
        {
            task.ContinueWith(t =>
            {
                var exception = t.Exception?.GetBaseException();

                if (exception == null)
                {
                    return;
                }

                if (IsExpectedTransportAbort(exception))
                {
                    OnChildLogString?.Invoke(this,
                        $"GET_calls_queued timed-out RPC ended after disconnect CallId={callId}, QueueDirNo='{queueDirNo ?? "<null>"}': {exception.Message}");
                    return;
                }

                OnChildLogString?.Invoke(this,
                    $"GET_calls_queued timed-out RPC faulted CallId={callId}, QueueDirNo='{queueDirNo ?? "<null>"}': {exception}");
            },
            System.Threading.CancellationToken.None,
            System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted | System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously,
            System.Threading.Tasks.TaskScheduler.Default);
        }

        /***********************************************************************************************************************/
        private static bool IsExpectedTransportAbort(Exception ex)
        /***********************************************************************************************************************/
        {
            if (ex == null)
            {
                return false;
            }

            if (ex is AggregateException aggregateException)
            {
                foreach (var innerException in aggregateException.Flatten().InnerExceptions)
                {
                    if (IsExpectedTransportAbort(innerException))
                    {
                        return true;
                    }
                }

                return false;
            }

            if (ex is WampConnectionBrokenException)
            {
                return true;
            }

            if (ex is System.Net.Sockets.SocketException socketException)
            {
                return socketException.SocketErrorCode == System.Net.Sockets.SocketError.ConnectionAborted ||
                       socketException.SocketErrorCode == System.Net.Sockets.SocketError.ConnectionReset ||
                       socketException.SocketErrorCode == System.Net.Sockets.SocketError.OperationAborted;
            }

            if (ex is IOException || ex is ObjectDisposedException)
            {
                return IsExpectedTransportAbort(ex.InnerException);
            }

            return IsExpectedTransportAbort(ex.InnerException);
        }

        /***********************************************************************************************************************/
        private object GET_devices_gpos(string device_id, string id)
        {
            try
            {
                if (!TryGetActiveRpcCatalog(out var realmProxy, out _, "GET_devices_gpos"))
                {
                    return null;
                }

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

                realmProxy.RpcCatalog.Invoke(
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
                HandleExpectedRpcFailure("GET_devices_gpos", ex);
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
                if (!TryGetActiveRpcCatalog(out var realmProxy, out _, "GET_devices_gpis"))
                {
                    return null;
                }

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

                realmProxy.RpcCatalog.Invoke(
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
                HandleExpectedRpcFailure("GET_devices_gpis", ex);
                return null;
            }
        }


        /***********************************************************************************************************************/
        private object GET_PlatformVersion()
        /***********************************************************************************************************************/
        {
            try
            {
                if (!TryGetActiveServices(out _, out var svc, out _, "GET_PlatformVersion"))
                {
                    return null;
                }

                return svc.GetPlatformVersion();
            }
            catch (Exception ex)
            {
                HandleExpectedRpcFailure("GET_PlatformVersion", ex);
                return null;
            }
        }

        /***********************************************************************************************************************/
        private object GET_groups(string dirno, bool verbose)
        /***********************************************************************************************************************/
        {
            try
            {
                if (!TryGetActiveServices(out _, out var svc, out _, "GET_groups"))
                {
                    return null;
                }

                return svc.GET_groups(dirno, verbose);

            }
            catch (Exception ex)
            {
                HandleExpectedRpcFailure("GET_groups", ex);
                return null;
            }
        }

        /***********************************************************************************************************************/
        private object GET_audio_messages()
        /***********************************************************************************************************************/
        {
            try
            {
                if (!TryGetActiveServices(out _, out var svc, out _, "GET_audio_messages"))
                {
                    return null;
                }

                return svc.GET_audio_messages();

            }
            catch (Exception ex)
            {
                HandleExpectedRpcFailure("GET_audio_messages", ex);
                return null;
            }
        }

        /***********************************************************************************************************************/
        private object GET_directories(string dirno)
        /***********************************************************************************************************************/
        {
            try
            {
                if (!TryGetActiveServices(out _, out var svc, out _, "GET_directories"))
                {
                    return null;
                }

                return svc.GET_directories(dirno);

            }
            catch (Exception ex)
            {
                HandleExpectedRpcFailure("GET_directories", ex);
                return null;
            }
        }
    }
    #endregion call functions from server
}

