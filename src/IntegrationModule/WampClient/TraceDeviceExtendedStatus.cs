using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WampSharp.Core.Serialization;
using WampSharp.V2.Client;
using WampSharp.V2.Core.Contracts;
using WampSharp.V2.PubSub;

namespace Wamp.Client
{

    public partial class WampClient
    {
        private readonly object _traceDeviceExtendedStatusEventGate = new object();

        private TracerDeviceExtendedStatusEvent tracerDeviceExtendedStatusEvent;
        private IAsyncDisposable tracerDeviceExtendedStatusEventDisposable;

        /// <summary>
        /// Occurs when extended device test status information is published by the WAMP backend.
        /// </summary>
        public event EventHandler<wamp_device_extended_status> OnWampDeviceExtendedStatusEvent;

        /// <summary>
        /// Subscribes to device extended status notifications from the active WAMP realm.
        /// </summary>
        public async void TraceDeviceExtendedStatusEvent()
        {
            await TraceDeviceExtendedStatusEventAsync().ConfigureAwait(false);
        }

        internal async Task TraceDeviceExtendedStatusEventAsync()
        {
            try
            {
                lock (_traceDeviceExtendedStatusEventGate)
                {
                    if (tracerDeviceExtendedStatusEventDisposable != null)
                        return;
                }

                if (_wampRealmProxy == null)
                {
                    OnChildLogString?.Invoke(this, "TraceDeviceExtendedStatusEvent skipped. WAMP realm proxy is not available.");
                    return;
                }

                OnChildLogString?.Invoke(this,
                    "TraceDeviceExtendedStatusEvent starting subscription on current realm. " + GetConnectionDebugStateForRealm(_wampRealmProxy));

                IWampTopicProxy topicProxy = _wampRealmProxy.TopicContainer.GetTopicByUri(TraceDeviceExtendedStatus);

                var tracer = new TracerDeviceExtendedStatusEvent();
                tracer.OnDeviceExtendedStatusEvent += TracerDeviceExtendedStatusEvent_OnRegistrationEvent;
                tracer.OnDebugString += TracerDeviceExtendedStatusEvent_OnDebugString;

                IAsyncDisposable subscription = await topicProxy
                    .Subscribe(tracer, new SubscribeOptions())
                    .ConfigureAwait(false);

                lock (_traceDeviceExtendedStatusEventGate)
                {
                    if (tracerDeviceExtendedStatusEventDisposable != null)
                    {
                        tracer.OnDeviceExtendedStatusEvent -= TracerDeviceExtendedStatusEvent_OnRegistrationEvent;
                        tracer.OnDebugString -= TracerDeviceExtendedStatusEvent_OnDebugString;
                        subscription.DisposeAsync().AsTask().GetAwaiter().GetResult();
                        return;
                    }

                    tracerDeviceExtendedStatusEvent = tracer;
                    tracerDeviceExtendedStatusEventDisposable = subscription;

                    OnChildLogString?.Invoke(this,
                        "TraceDeviceExtendedStatusEvent subscription established. " + GetConnectionDebugStateForRealm(_wampRealmProxy));
                }
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception in TraceDeviceExtendedStatusEvent: " + ex);
            }
        }

        private void TracerDeviceExtendedStatusEvent_OnDebugString(object sender, string e)
        {
            OnChildLogString?.Invoke(this, "Device Extended Status Event: " + e);
        }

        private void TracerDeviceExtendedStatusEvent_OnRegistrationEvent(object sender, wamp_device_extended_status deviceStat)
        {
            OnWampDeviceExtendedStatusEvent?.Invoke(this, deviceStat);
        }

        /// <summary>
        /// Disposes the device extended status subscription and detaches the internal tracer handlers.
        /// </summary>
        public void TraceDeviceExtendedStatusEventDispose()
        {
            TracerDeviceExtendedStatusEvent tracer = null;
            IAsyncDisposable subscription = null;

            lock (_traceDeviceExtendedStatusEventGate)
            {
                tracer = tracerDeviceExtendedStatusEvent;
                subscription = tracerDeviceExtendedStatusEventDisposable;

                tracerDeviceExtendedStatusEvent = null;
                tracerDeviceExtendedStatusEventDisposable = null;
            }

            if (tracer != null)
            {
                tracer.OnDeviceExtendedStatusEvent -= TracerDeviceExtendedStatusEvent_OnRegistrationEvent;
                tracer.OnDebugString -= TracerDeviceExtendedStatusEvent_OnDebugString;
            }

            if (subscription == null)
                return;

            try
            {
                subscription.DisposeAsync().AsTask().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception disposing TraceDeviceExtendedStatusEvent subscription: " + ex);
            }
        }

        /// <summary>
        /// Determines whether the device extended status subscription is currently active.
        /// </summary>
        /// <returns><see langword="true"/> when the subscription is active; otherwise, <see langword="false"/>.</returns>
        public bool TraceDeviceExtendedStatusIsEnabled()
        {
            lock (_traceDeviceExtendedStatusEventGate)
            {
                return tracerDeviceExtendedStatusEventDisposable != null;
            }
        }

        internal class TracerDeviceExtendedStatusEvent : IWampRawTopicClientSubscriber
        {
            public event EventHandler<wamp_device_extended_status> OnDeviceExtendedStatusEvent;
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

                var deviceStatus = Newtonsoft.Json.JsonConvert.DeserializeObject<wamp_device_extended_status>(json);

                if (deviceStatus != null)
                    OnDeviceExtendedStatusEvent?.Invoke(this, deviceStatus);
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
