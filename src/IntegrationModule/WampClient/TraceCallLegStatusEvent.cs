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
        private readonly object _traceCallLegEventGate = new object();

        private TracerCallLegEvent tracerCallLegEvent;
        private IAsyncDisposable tracerCallLegEventDisposable;

        /// <summary>
        /// Occurs when the state of a call leg changes on the WAMP event stream.
        /// </summary>
        public event EventHandler<wamp_call_leg_element> OnWampCallLegStatusEvent;

        /// <summary>
        /// Subscribes to call leg status notifications from the active WAMP realm.
        /// </summary>
        public async void TraceCallLegEvent()
        {
            try
            {
                lock (_traceCallLegEventGate)
                {
                    if (tracerCallLegEventDisposable != null)
                        return;
                }

                if (_wampRealmProxy == null)
                {
                    OnChildLogString?.Invoke(this, "TraceCallLegEvent skipped. WAMP realm proxy is not available.");
                    return;
                }

                IWampTopicProxy topicProxy = _wampRealmProxy.TopicContainer.GetTopicByUri(TraceWampCallLeg);

                var tracer = new TracerCallLegEvent();
                tracer.OnCallLegEvent += TracerCallLegEvent_OnCallLegEvent;
                tracer.OnDebugString += TracerCallLegEvent_OnDebugString;

                IAsyncDisposable subscription = await topicProxy
                    .Subscribe(tracer, new SubscribeOptions())
                    .ConfigureAwait(false);

                lock (_traceCallLegEventGate)
                {
                    if (tracerCallLegEventDisposable != null)
                    {
                        tracer.OnCallLegEvent -= TracerCallLegEvent_OnCallLegEvent;
                        tracer.OnDebugString -= TracerCallLegEvent_OnDebugString;
                        subscription.DisposeAsync().AsTask().GetAwaiter().GetResult();
                        return;
                    }

                    tracerCallLegEvent = tracer;
                    tracerCallLegEventDisposable = subscription;
                }
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception in TraceCallLegEvent: " + ex);
            }
        }

        private void TracerCallLegEvent_OnDebugString(object sender, string e)
        {
            OnChildLogString?.Invoke(this, "Call Leg Subscription Event: " + e);
        }

        private void TracerCallLegEvent_OnCallLegEvent(object sender, wamp_call_leg_element callQueueUpd)
        {
            OnWampCallLegStatusEvent?.Invoke(this, callQueueUpd);
        }

        /// <summary>
        /// Disposes the call leg status subscription and detaches the internal tracer handlers.
        /// </summary>
        public void TraceCallLegEventDispose()
        {
            TracerCallLegEvent tracer = null;
            IAsyncDisposable subscription = null;

            lock (_traceCallLegEventGate)
            {
                tracer = tracerCallLegEvent;
                subscription = tracerCallLegEventDisposable;

                tracerCallLegEvent = null;
                tracerCallLegEventDisposable = null;
            }

            if (tracer != null)
            {
                tracer.OnCallLegEvent -= TracerCallLegEvent_OnCallLegEvent;
                tracer.OnDebugString -= TracerCallLegEvent_OnDebugString;
            }

            if (subscription == null)
                return;

            try
            {
                subscription.DisposeAsync().AsTask().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception disposing TraceCallLegEvent subscription: " + ex);
            }
        }

        /// <summary>
        /// Determines whether the call leg status subscription is currently active.
        /// </summary>
        /// <returns><see langword="true"/> when the subscription is active; otherwise, <see langword="false"/>.</returns>
        public bool TraceCallLegEventIsEnabled()
        {
            lock (_traceCallLegEventGate)
            {
                return tracerCallLegEventDisposable != null;
            }
        }

        internal class TracerCallLegEvent : IWampRawTopicClientSubscriber
        {
            public event EventHandler<wamp_call_leg_element> OnCallLegEvent;
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

                var callQueueUpdate = Newtonsoft.Json.JsonConvert.DeserializeObject<wamp_call_leg_element>(json);

                if (callQueueUpdate != null)
                    OnCallLegEvent?.Invoke(this, callQueueUpdate);
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
