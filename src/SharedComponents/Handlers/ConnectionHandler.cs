using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Wamp.Client;
using Zenitel.IntegrationModule.REST;
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
        private RestClient _rest; // Field for managing REST authentication

        /// <summary>
        /// Maximum number of reconnection attempts.
        /// </summary>
        
        private int _maxReconnect = 50;
        private int _postConnectWarmupGeneration = 0;
        private CancellationTokenSource _postConnectWarmupCancellationTokenSource;
        private static readonly TimeSpan WarmupStepDelay = TimeSpan.FromMilliseconds(150);
        private bool _postConnectRetrievalTriggered = false;

        /// <summary>
        /// Synchronization lock to prevent concurrent connection operations.
        /// </summary>
       
        private object _lockObject = new object();

        private void LogConnectionDebug(string message)
        {
            System.Diagnostics.Debug.WriteLine("ConnectionHandler: " + message);
        }

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

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionHandler"/> class with REST client support.
        /// </summary>
        /// <param name="events">Reference to the events object handling connection-related events.</param>
        /// <param name="wamp">Reference to the WAMP client for server communication.</param>
        /// <param name="rest">Reference to the REST client for REST API communication.</param>
        /// <param name="configuration">Reference to the configuration settings.</param>
        /// <param name="parentIpAddress">The IP address of the parent device.</param>
        
        public ConnectionHandler(ref Events events, ref WampClient wamp, ref RestClient rest, ref Configuration configuration, string parentIpAddress)
        {
            _events = events;
            _wamp = wamp;
            _rest = rest ?? throw new ArgumentNullException(nameof(rest));

            // Apply configuration settings to the WAMP client.
            _wamp.WampServerAddr = configuration.ServerAddr;
            _wamp.WampPort = configuration.Port;
            _wamp.UserName = configuration.UserName;
            _wamp.Password = configuration.Password;

            // Apply configuration settings to the REST client.
            ConfigureRestClient(configuration);

            ParentIpAddress = parentIpAddress;

            // Register event handlers.
            _wamp.OnConnectChanged += HandleConnectionChangeEvent_Internal;
            _events.OnConfigurationChanged += HandleConfigurationChangeEvent;
        }
        
        /// <summary>
        /// Configures the REST client with credentials and server settings from the configuration.
        /// </summary>
        /// <param name="configuration">The configuration settings containing server address and credentials.</param>
        
        private void ConfigureRestClient(Configuration configuration)
        {
            if (_rest == null || configuration == null)
                return;

            _rest.ServerAddress = configuration.ServerAddr;
            _rest.UserName = configuration.UserName;
            _rest.Password = configuration.Password;
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
                LogConnectionDebug($"HandleConnectionChangeEvent_Internal received e={e}, WampIsConnected={_wamp.IsConnected}, IsReconnecting={IsReconnecting}, MaxReconnect={_maxReconnect}");

                this.IsConnected = _wamp.IsConnected;
                this.IsReconnecting = !_wamp.IsConnected;

                switch (this.IsConnected)
                {
                    case true:
                        CancelPostConnectWarmup();
                        var warmupGeneration = ++_postConnectWarmupGeneration;
                        _postConnectWarmupCancellationTokenSource = new CancellationTokenSource();
                        _postConnectRetrievalTriggered = false;

                        _ = BeginPostConnectWarmupAsync(warmupGeneration, _postConnectWarmupCancellationTokenSource.Token);

                        this.ResetRecconectionCounter();
                        break;
                    case false:
                        CancelPostConnectWarmup();
                        _postConnectRetrievalTriggered = false;
                        _events.OnConnectionChanged?.Invoke(this, false);
                        LogConnectionDebug($"Scheduling reconnect. MaxReconnect before decrement={_maxReconnect}");
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
        /// Handles configuration changes by updating the WAMP and REST client settings.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="config">The updated configuration settings.</param>
        
        private void HandleConfigurationChangeEvent(object sender, ConnectPro.Configuration config)
        {
            this._wamp.WampServerAddr = config.ServerAddr;
            this._wamp.WampPort = config.Port;
            this._wamp.UserName = config.UserName;
            this._wamp.Password = config.Password;

            // Update REST client configuration if available
            if (_rest != null)
            {
                ConfigureRestClient(config);
            }
        }

        #endregion

        #region Connection Management

        /// <summary>
        /// Opens a new connection to both WAMP and REST clients.
        /// </summary>
        public void OpenConnection()
        {
            lock (_lockObject)
            {
                LogConnectionDebug($"OpenConnection invoked. WampServer={_wamp.WampServerAddr}, Port={_wamp.WampPort}, MaxReconnect={_maxReconnect}");

                _wamp.Start();
                IsReconnecting = true;

                // Authenticate REST client asynchronously if available
                if (_rest != null)
                {
                    _ = AuthenticateRestAsync();
                }
            }
        }

        /// <summary>
        /// Authenticates the REST client asynchronously.
        /// </summary>
        private async Task AuthenticateRestAsync()
        {
            try
            {
                if (_rest != null)
                {
                    bool authSuccess = await _rest.AuthenticateAsync(CancellationToken.None);
                    if (authSuccess)
                    {
                        System.Diagnostics.Debug.WriteLine("REST client authenticated successfully");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("REST client authentication failed");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error authenticating REST client: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Attempts to reconnect to the WAMP server, respecting the maximum reconnection limit.
        /// </summary>
        
        public void Recconect()
        {
            lock (_lockObject)
            {
                CancelPostConnectWarmup();

                if (_maxReconnect > 0)
                {
                    LogConnectionDebug($"Recconect executing. MaxReconnect={_maxReconnect}. Calling Stop() then Start().");

                    _wamp.Stop();
                    _wamp.Start();
                    _maxReconnect--;
                }
                else
                {
                    LogConnectionDebug("Recconect skipped because MaxReconnect reached 0.");
                }
            }
        }

        /// <summary>
        /// Initiates a reconnect attempt asynchronously.
        /// </summary>
        /// <returns>A task that represents the reconnect request.</returns>
        public async Task RecoonectAsync()
        {
            Recconect();
            await Task.CompletedTask;
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
            CancelPostConnectWarmup();
            LogConnectionDebug("Dispose invoked. Calling _wamp.Stop().");
            _wamp.Stop();
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private async Task BeginPostConnectWarmupAsync(int generation, CancellationToken cancellationToken)
        {
            try
            {
                if (!IsWarmupGenerationCurrent(generation))
                {
                    LogConnectionDebug($"Post-connect warm-up aborted before start because generation is stale. Generation={generation}, CurrentGeneration={_postConnectWarmupGeneration}");
                    return;
                }

                LogConnectionDebug($"Post-connect warm-up starting. Generation={generation}");

                await RunWarmupStepAsync("TraceCallEvent", () => _wamp.TraceCallEventAsync(), generation, cancellationToken);
                await RunWarmupStepAsync("TraceCallLegEvent", () => _wamp.TraceCallLegEventAsync(), generation, cancellationToken);
                await RunWarmupStepAsync("TraceDeviceRegistrationEvent", () => _wamp.TraceDeviceRegistrationEventAsync(), generation, cancellationToken);
                await RunWarmupStepAsync("TraceOpenDoorEvent", () => _wamp.TraceOpenDoorEventAsync(), generation, cancellationToken);
                await RunWarmupStepAsync("TraceDeviceExtendedStatusEvent", () => _wamp.TraceDeviceExtendedStatusEventAsync(), generation, cancellationToken);
                await RunWarmupStepAsync("TraceAudioEventDetection", () => _wamp.TraceAudioEventDetectionAsync(), generation, cancellationToken);
                await RunWarmupStepAsync("TraceAudioDataReceiving", () => _wamp.TraceAudioDataReceivingAsync(), generation, cancellationToken);
                await RunWarmupStepAsync("TraceAudioDetectorAlive", () => _wamp.TraceAudioDetectorAliveAsync(), generation, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(WarmupStepDelay, cancellationToken).ConfigureAwait(false);

                LogConnectionDebug($"Post-connect warm-up signaling stable connected state. Generation={generation}");
                _events.OnConnectionChanged?.Invoke(this, true);

                await Task.Delay(WarmupStepDelay, cancellationToken).ConfigureAwait(false);

                if (!_postConnectRetrievalTriggered)
                {
                    _postConnectRetrievalTriggered = true;
                    LogConnectionDebug($"Post-connect warm-up triggering initial device retrieval. Generation={generation}");
                    _events.OnDeviceRetrievalStart?.Invoke(this, EventArgs.Empty);
                }

                LogConnectionDebug($"Post-connect warm-up completed. Generation={generation}");
            }
            catch (OperationCanceledException)
            {
                LogConnectionDebug($"Post-connect warm-up canceled. Generation={generation}");
            }
            catch (Exception ex)
            {
                LogConnectionDebug($"Post-connect warm-up failed. Generation={generation}, Exception={ex}");
            }
        }

        private async Task RunWarmupStepAsync(string stepName, Func<Task> action, int generation, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!IsWarmupGenerationCurrent(generation) || !_wamp.IsConnected)
            {
                LogConnectionDebug($"Warm-up step canceled before start: {stepName}. RequestedGeneration={generation}, CurrentGeneration={_postConnectWarmupGeneration}, WampIsConnected={_wamp.IsConnected}");
                throw new OperationCanceledException(cancellationToken);
            }

            LogConnectionDebug($"Warm-up step starting: {stepName}. Generation={generation}, WampIsConnected={_wamp.IsConnected}");
            await action().ConfigureAwait(false);

            if (!IsWarmupGenerationCurrent(generation) || !_wamp.IsConnected)
            {
                LogConnectionDebug($"Warm-up step canceled after completion: {stepName}. RequestedGeneration={generation}, CurrentGeneration={_postConnectWarmupGeneration}, WampIsConnected={_wamp.IsConnected}");
                throw new OperationCanceledException(cancellationToken);
            }

            LogConnectionDebug($"Warm-up step completed: {stepName}. Generation={generation}, WampIsConnected={_wamp.IsConnected}");

            await Task.Delay(WarmupStepDelay, cancellationToken).ConfigureAwait(false);
        }

        private bool IsWarmupGenerationCurrent(int generation)
        {
            lock (_lockObject)
            {
                return generation == _postConnectWarmupGeneration && _postConnectWarmupCancellationTokenSource != null;
            }
        }

        private void CancelPostConnectWarmup()
        {
            lock (_lockObject)
            {
                if (_postConnectWarmupCancellationTokenSource == null)
                    return;

                LogConnectionDebug($"CancelPostConnectWarmup invoked. CurrentGeneration={_postConnectWarmupGeneration}");

                try
                {
                    _postConnectWarmupCancellationTokenSource.Cancel();
                }
                catch (ObjectDisposedException)
                {
                }
                finally
                {
                    _postConnectWarmupCancellationTokenSource.Dispose();
                    _postConnectWarmupCancellationTokenSource = null;
                }
            }
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
