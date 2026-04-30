using System;
using System.Collections.Generic;
using WampSharp.Core.Serialization;
using WampSharp.V2.Client;
using WampSharp.V2.Core.Contracts;
using WampSharp.V2.PubSub;

namespace Wamp.Client
{

    public partial class WampClient
    {
        private readonly object _traceDeviceRegistrationEventGate = new object();

        private TracerDeviceRegistrationEvent tracerDeviceRegistrationEvent;
        private IAsyncDisposable tracerDeviceRegistrationEventDisposable;

        /// <summary>
        /// Occurs when a device registration update is received from the WAMP event stream.
        /// </summary>
        public event EventHandler<wamp_device_registration_element> OnWampDeviceRegistrationEvent;

        /// <summary>
        /// Subscribes to device registration notifications from the active WAMP realm.
        /// </summary>
        public async void TraceDeviceRegistrationEvent()
        {
            try
            {
                lock (_traceDeviceRegistrationEventGate)
                {
                    if (tracerDeviceRegistrationEventDisposable != null)
                        return;
                }

                if (_wampRealmProxy == null)
                {
                    OnChildLogString?.Invoke(this, "TraceDeviceRegistrationEvent skipped. WAMP realm proxy is not available.");
                    return;
                }

                IWampTopicProxy topicProxy = _wampRealmProxy.TopicContainer.GetTopicByUri(TraceWampRegisteredDevices);

                var tracer = new TracerDeviceRegistrationEvent();
                tracer.OnRegistrationEvent += TracerDeviceRegistrationEvent_OnRegistrationEvent;
                tracer.OnDebugString += TracerDeviceRegistrationEvent_OnDebugString;

                IAsyncDisposable subscription = await topicProxy
                    .Subscribe(tracer, new SubscribeOptions())
                    .ConfigureAwait(false);

                lock (_traceDeviceRegistrationEventGate)
                {
                    if (tracerDeviceRegistrationEventDisposable != null)
                    {
                        tracer.OnRegistrationEvent -= TracerDeviceRegistrationEvent_OnRegistrationEvent;
                        tracer.OnDebugString -= TracerDeviceRegistrationEvent_OnDebugString;
                        subscription.DisposeAsync().AsTask().GetAwaiter().GetResult();
                        return;
                    }

                    tracerDeviceRegistrationEvent = tracer;
                    tracerDeviceRegistrationEventDisposable = subscription;
                }
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception in TraceDeviceRegistrationEvent: " + ex);
            }
        }

        private void TracerDeviceRegistrationEvent_OnDebugString(object sender, string e)
        {
            OnChildLogString?.Invoke(this, "Device Registration Subscription Event: " + e);
        }

        private void TracerDeviceRegistrationEvent_OnRegistrationEvent(object sender, wamp_device_registration_element regUpd)
        {
            OnWampDeviceRegistrationEvent?.Invoke(this, regUpd);
        }

        /// <summary>
        /// Disposes the device registration subscription and detaches the internal tracer handlers.
        /// </summary>
        public void TraceDeviceRegistrationEventDispose()
        {
            TracerDeviceRegistrationEvent tracer = null;
            IAsyncDisposable subscription = null;

            lock (_traceDeviceRegistrationEventGate)
            {
                tracer = tracerDeviceRegistrationEvent;
                subscription = tracerDeviceRegistrationEventDisposable;

                tracerDeviceRegistrationEvent = null;
                tracerDeviceRegistrationEventDisposable = null;
            }

            if (tracer != null)
            {
                tracer.OnRegistrationEvent -= TracerDeviceRegistrationEvent_OnRegistrationEvent;
                tracer.OnDebugString -= TracerDeviceRegistrationEvent_OnDebugString;
            }

            if (subscription == null)
                return;

            try
            {
                subscription.DisposeAsync().AsTask().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception disposing TraceDeviceRegistrationEvent subscription: " + ex);
            }
        }

        /// <summary>
        /// Determines whether the device registration subscription is currently active.
        /// </summary>
        /// <returns><see langword="true"/> when the subscription is active; otherwise, <see langword="false"/>.</returns>
        public bool TraceDeviceRegistrationIsEnabled()
        {
            lock (_traceDeviceRegistrationEventGate)
            {
                return tracerDeviceRegistrationEventDisposable != null;
            }
        }

        internal class TracerDeviceRegistrationEvent : IWampRawTopicClientSubscriber
        {
            public event EventHandler<wamp_device_registration_element> OnRegistrationEvent;
            public event EventHandler<string> OnDebugString;

            public void Event<TMessage>(
                IWampFormatter<TMessage> formatter,
                long publicationId,
                EventDetails details)
            {
                OnDebugString?.Invoke(this, "Got event with publication id: " + publicationId);
            }

            public void Event<TMessage>(
                IWampFormatter<TMessage> formatter,
                long publicationId,
                EventDetails details,
                TMessage[] arguments)
            {
                if (arguments == null || arguments.Length == 0 || arguments[0] == null)
                    return;

                string json = arguments[0].ToString();
                OnDebugString?.Invoke(this, json);

                var regUpdate = Newtonsoft.Json.JsonConvert.DeserializeObject<wamp_device_registration_element>(json);

                if (regUpdate != null)
                    OnRegistrationEvent?.Invoke(this, regUpdate);
            }

            public void Event<TMessage>(
                IWampFormatter<TMessage> formatter,
                long publicationId,
                EventDetails details,
                TMessage[] arguments,
                IDictionary<string, TMessage> argumentsKeywords)
            {
                OnDebugString?.Invoke(this, "Event with argumentsKeywords is not supported.");
            }
        }
    }
}
