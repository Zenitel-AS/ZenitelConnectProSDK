using System;
using System.Threading;
using System.Threading.Tasks;

namespace Wamp.Client
{
    public partial class WampClient
    {
        private readonly SemaphoreSlim _calleeRegistrationGate = new SemaphoreSlim(1, 1);
        private IAsyncDisposable _calleeRegistrationDisposable;
        private object _calleeRegistrationRealmProxy;
        private long _calleeRegistrationGeneration;

        /// <summary>
        /// Registers the SDK's callee services against the active WAMP realm so server-side calls can be handled.
        /// </summary>
        public async Task RegisterCalleeServices()
        {
            try
            {
                OnChildLogString?.Invoke(this, "RegisterCalleeServices() invoked. " + GetConnectionDebugState());

                if (!TryGetActiveRealmProxy(out var realmProxy, out var generation))
                {
                    OnChildLogString?.Invoke(this, "RegisterCalleeServices skipped. WAMP realm proxy is not available.");
                    return;
                }

                OnChildLogString?.Invoke(this,
                    $"RegisterCalleeServices acquired active realm proxy. Generation={generation}, RealmProxyHash={realmProxy.GetHashCode()}, ServicesReady={realmProxy.Services != null}. " + GetConnectionDebugState());

                if (realmProxy == null || realmProxy.Services == null)
                {
                    OnChildLogString?.Invoke(this, "RegisterCalleeServices skipped. WAMP realm proxy is not available.");
                    return;
                }

                await _calleeRegistrationGate.WaitAsync().ConfigureAwait(false);

                try
                {
                    if (_calleeRegistrationDisposable != null &&
                        ReferenceEquals(_calleeRegistrationRealmProxy, realmProxy) &&
                        _calleeRegistrationGeneration == generation)
                    {
                        OnChildLogString?.Invoke(this, $"RegisterCalleeServices skipped. Services already registered for Generation={generation}, RealmProxyHash={realmProxy.GetHashCode()}.");
                        return;
                    }

                    if (_calleeRegistrationDisposable != null)
                    {
                        OnChildLogString?.Invoke(this, $"Disposing stale callee services registration before re-registering. ExistingGeneration={_calleeRegistrationGeneration}, NewGeneration={generation}.");

                        await _calleeRegistrationDisposable.DisposeAsync().ConfigureAwait(false);

                        _calleeRegistrationDisposable = null;
                        _calleeRegistrationRealmProxy = null;
                    }

                    IArgumentsService instance = new ArgumentsService();

                    _calleeRegistrationDisposable =
                        await realmProxy.Services.RegisterCallee(instance).ConfigureAwait(false);

                    _calleeRegistrationRealmProxy = realmProxy;
                    _calleeRegistrationGeneration = generation;

                    OnChildLogString?.Invoke(this, $"RegisterCalleeServices() completed successfully. Generation={generation}, RealmProxyHash={realmProxy.GetHashCode()}.");
                }
                finally
                {
                    _calleeRegistrationGate.Release();
                }
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception in RegisterCalleeServices: " + ex + ". " + GetConnectionDebugState());
            }
        }

        private async Task RegisterCalleeServicesDisposeAsync()
        {
            try
            {
                var activeGeneration = CaptureActiveChannelContext()?.Generation ?? 0;

                OnChildLogString?.Invoke(this,
                    $"RegisterCalleeServicesDisposeAsync invoked. ActiveGeneration={activeGeneration}, RegisteredGeneration={_calleeRegistrationGeneration}. " + GetConnectionDebugState());

                await _calleeRegistrationGate.WaitAsync().ConfigureAwait(false);

                try
                {
                    if (_calleeRegistrationDisposable == null)
                        return;

                    if (activeGeneration != 0 && _calleeRegistrationGeneration == activeGeneration)
                    {
                        OnChildLogString?.Invoke(this, $"Skipping callee registration dispose for the active WAMP generation {activeGeneration}.");
                        return;
                    }

                    await _calleeRegistrationDisposable.DisposeAsync().ConfigureAwait(false);

                    _calleeRegistrationDisposable = null;
                    _calleeRegistrationRealmProxy = null;
                    _calleeRegistrationGeneration = 0;

                    OnChildLogString?.Invoke(this, $"Callee services registration disposed. DisposedGeneration={_calleeRegistrationGeneration}, ActiveGeneration={activeGeneration}.");
                }
                finally
                {
                    _calleeRegistrationGate.Release();
                }
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception disposing callee services registration: " + ex + ". " + GetConnectionDebugState());
            }
        }
    }
}