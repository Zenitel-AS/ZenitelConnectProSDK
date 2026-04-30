using System;
using System.Threading.Tasks;

namespace Wamp.Client
{
    public partial class WampClient
    {
        private readonly object _calleeRegistrationGate = new object();
        private IAsyncDisposable _calleeRegistrationDisposable;

        /// <summary>
        /// Registers the SDK's callee services against the active WAMP realm so server-side calls can be handled.
        /// </summary>
        /// <returns>A task that represents the asynchronous registration operation.</returns>
        public async Task RegisterCalleeServices()
        {
            try
            {
                OnChildLogString?.Invoke(this, "RegisterCalleeServices() invoked.");

                if (_wampRealmProxy == null || _wampRealmProxy.Services == null)
                {
                    OnChildLogString?.Invoke(this, "RegisterCalleeServices skipped. WAMP realm proxy is not available.");
                    return;
                }

                lock (_calleeRegistrationGate)
                {
                    if (_calleeRegistrationDisposable != null)
                    {
                        OnChildLogString?.Invoke(this, "RegisterCalleeServices skipped. Services are already registered.");
                        return;
                    }
                }

                IArgumentsService instance = new ArgumentsService();
                IAsyncDisposable registration = await _wampRealmProxy.Services.RegisterCallee(instance);

                lock (_calleeRegistrationGate)
                {
                    if (_calleeRegistrationDisposable != null)
                    {
                        registration.DisposeAsync();
                        return;
                    }

                    _calleeRegistrationDisposable = registration;
                }

                OnChildLogString?.Invoke(this, "RegisterCalleeServices() completed successfully.");
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception in RegisterCalleeServices: " + ex);
            }
        }

        private void RegisterCalleeServicesDispose()
        {
            IAsyncDisposable registration = null;

            lock (_calleeRegistrationGate)
            {
                registration = _calleeRegistrationDisposable;
                _calleeRegistrationDisposable = null;
            }

            if (registration == null)
                return;

            try
            {
                registration.DisposeAsync().AsTask().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception disposing callee services registration: " + ex);
            }
        }
    }
}