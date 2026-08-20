using System;
using System.Net;

namespace Wamp.Client
{
    public partial class WampClient : IDisposable
    {
        private bool _disposed;
        private readonly object _disposeLock = new object();

        /// <summary>
        /// Releases WAMP subscriptions, timers, connection resources, and other managed state owned by the client.
        /// </summary>
        public void Dispose()
        {
            lock (_disposeLock)
            {
                if (_disposed)
                    return;

                _disposed = true;
            }

            lock (_connectionGate)
            {
                _isStopping = true;
            }

            DisposeTraceSubscriptions();
            DetachMonitorEvents();
            DisposeTimers();
            CloseChannel();
            ClearReferences();
            RemoveCertificateCallback();

            GC.SuppressFinalize(this);
        }

        private void DisposeTraceSubscriptions()
        {
            SafeDisposeStep("RegisterCalleeServicesDispose", () => RegisterCalleeServicesDisposeAsync().GetAwaiter().GetResult());
            SafeDisposeStep("TraceAudioDataReceivingDispose", TraceAudioDataReceivingDispose);
            SafeDisposeStep("TraceAudioDetectorAliveDispose", TraceAudioDetectorAliveDispose);
            SafeDisposeStep("TraceAudioEventDetectionDispose", TraceAudioEventDetectionDispose);
            SafeDisposeStep("TraceCallLegEventDispose", TraceCallLegEventDispose);
            SafeDisposeStep("TraceCallEventDispose", TraceCallEventDispose);
            SafeDisposeStep("TraceDeviceExtendedStatusEventDispose", TraceDeviceExtendedStatusEventDispose);
            SafeDisposeStep("TraceDeviceGPIStatusEventDispose", TraceDeviceGPIStatusEventDispose);
            SafeDisposeStep("TraceDeviceGPOStatusEventDispose", TraceDeviceGPOStatusEventDispose);
            SafeDisposeStep("TraceDeviceRegistrationEventDispose", TraceDeviceRegistrationEventDispose);
            SafeDisposeStep("TraceOpenDoorEventDispose", TraceOpenDoorEventDispose);
        }

        private void DetachMonitorEvents()
        {
            try
            {
                if (_wampRealmProxy != null && _wampRealmProxy.Monitor != null)
                {
                    _wampRealmProxy.Monitor.ConnectionEstablished -= Monitor_ConnectionEstablished;
                    _wampRealmProxy.Monitor.ConnectionError -= Monitor_ConnectionError;
                    _wampRealmProxy.Monitor.ConnectionBroken -= Monitor_ConnectionBroken;
                }
            }
            catch (Exception ex)
            {
                LogDisposeException("DetachMonitorEvents", ex);
            }
        }

        private void DisposeTimers()
        {
            try
            {
                StopReconnect();
            }
            catch (Exception ex)
            {
                LogDisposeException("StopReconnect", ex);
            }

            try
            {
                if (RenewAccessTokenTimer != null)
                {
                    RenewAccessTokenTimer.Dispose();
                    RenewAccessTokenTimer = null;
                }

                RenewAccessTokenRequested = false;
            }
            catch (Exception ex)
            {
                LogDisposeException("RenewAccessTokenTimer.Dispose", ex);
            }
        }

        private void CloseChannel()
        {
            try
            {
                if (_wampChannel != null)
                {
                    _wampChannel.Close();
                    _wampChannel = null;
                }
            }
            catch (Exception ex)
            {
                LogDisposeException("CloseChannel", ex);
            }
        }

        private void ClearReferences()
        {
            _wampRealmProxy = null;
            _wampAuthenticator = null;
            IsConnected = false;
        }

        private void RemoveCertificateCallback()
        {
            try
            {
                RemoveCertificateValidationCallback();
            }
            catch (Exception ex)
            {
                LogDisposeException("RemoveCertificateCallback", ex);
            }
        }

        private void SafeDisposeStep(string name, Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                LogDisposeException(name, ex);
            }
        }

        private void LogDisposeException(string operation, Exception ex)
        {
            try
            {
                OnChildLogString?.Invoke(
                    this,
                    "Exception during WampClient.Dispose step '" + operation + "': " + ex);
            }
            catch
            {
                // Never allow logging failure to break Dispose().
            }
        }

        // WampClient.Dispose.cs (or WampClient.cs) — reset event subscriptions so they re-arm on the next connect
        internal void ResetTraceSubscriptionsForReconnect()
        {
            SafeDisposeStep("TraceAudioDataReceivingDispose", TraceAudioDataReceivingDispose);
            SafeDisposeStep("TraceAudioDetectorAliveDispose", TraceAudioDetectorAliveDispose);
            SafeDisposeStep("TraceAudioEventDetectionDispose", TraceAudioEventDetectionDispose);
            SafeDisposeStep("TraceCallLegEventDispose", TraceCallLegEventDispose);
            SafeDisposeStep("TraceCallEventDispose", TraceCallEventDispose);
            SafeDisposeStep("TraceDeviceExtendedStatusEventDispose", TraceDeviceExtendedStatusEventDispose);
            SafeDisposeStep("TraceDeviceGPIStatusEventDispose", TraceDeviceGPIStatusEventDispose);
            SafeDisposeStep("TraceDeviceGPOStatusEventDispose", TraceDeviceGPOStatusEventDispose);
            SafeDisposeStep("TraceDeviceRegistrationEventDispose", TraceDeviceRegistrationEventDispose);
            SafeDisposeStep("TraceOpenDoorEventDispose", TraceOpenDoorEventDispose);
        }
    }
}