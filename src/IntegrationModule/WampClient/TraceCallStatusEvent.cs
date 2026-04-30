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
        private readonly object _traceCallEventGate = new object();

        private TracerCallEvent tracerCallEvent;
        private IAsyncDisposable tracerCallEventsDisposable;

        /// <summary>
        /// Occurs when the status of a call changes on the WAMP event stream.
        /// </summary>
        public event EventHandler<wamp_call_element> OnWampCallStatusEvent;

        /// <summary>
        /// Subscribes to call status notifications from the active WAMP realm.
        /// </summary>
        public async void TraceCallEvent()
        {
            try
            {
                lock (_traceCallEventGate)
                {
                    if (tracerCallEventsDisposable != null)
                        return;
                }

                if (_wampRealmProxy == null)
                {
                    OnChildLogString?.Invoke(this, "TraceCallEvent skipped. WAMP realm proxy is not available.");
                    return;
                }

                IWampTopicProxy topicProxy = _wampRealmProxy.TopicContainer.GetTopicByUri(TraceWampCalls);

                var tracer = new TracerCallEvent();
                tracer.OnCallEvent += TracerCallEvent_OnCallEvent;
                tracer.OnDebugString += TracerCallEvent_OnDebugString;

                IAsyncDisposable subscription = await topicProxy
                    .Subscribe(tracer, new SubscribeOptions())
                    .ConfigureAwait(false);

                lock (_traceCallEventGate)
                {
                    if (tracerCallEventsDisposable != null)
                    {
                        tracer.OnCallEvent -= TracerCallEvent_OnCallEvent;
                        tracer.OnDebugString -= TracerCallEvent_OnDebugString;
                        subscription.DisposeAsync().AsTask().GetAwaiter().GetResult();
                        return;
                    }

                    tracerCallEvent = tracer;
                    tracerCallEventsDisposable = subscription;
                }
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception in TraceCallEvent: " + ex);
            }
        }

        private void TracerCallEvent_OnDebugString(object sender, string e)
        {
            OnChildLogString?.Invoke(this, "Call Subscription Event: " + e);
        }

        private void TracerCallEvent_OnCallEvent(object sender, wamp_call_element callUpd)
        {
            OnWampCallStatusEvent?.Invoke(this, callUpd);
        }

        /// <summary>
        /// Disposes the call status subscription and detaches the internal tracer handlers.
        /// </summary>
        public void TraceCallEventDispose()
        {
            TracerCallEvent tracer = null;
            IAsyncDisposable subscription = null;

            lock (_traceCallEventGate)
            {
                tracer = tracerCallEvent;
                subscription = tracerCallEventsDisposable;

                tracerCallEvent = null;
                tracerCallEventsDisposable = null;
            }

            if (tracer != null)
            {
                tracer.OnCallEvent -= TracerCallEvent_OnCallEvent;
                tracer.OnDebugString -= TracerCallEvent_OnDebugString;
            }

            if (subscription == null)
                return;

            try
            {
                subscription.DisposeAsync().AsTask().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception disposing TraceCallEvent subscription: " + ex);
            }
        }

        /// <summary>
        /// Determines whether the call status subscription is currently active.
        /// </summary>
        /// <returns><see langword="true"/> when the subscription is active; otherwise, <see langword="false"/>.</returns>
        public bool TraceCallEventIsEnabled()
        {
            lock (_traceCallEventGate)
            {
                return tracerCallEventsDisposable != null;
            }
        }

        internal class TracerCallEvent : IWampRawTopicClientSubscriber
        {
            public event EventHandler<wamp_call_element> OnCallEvent;
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

                var callUpdate = Newtonsoft.Json.JsonConvert.DeserializeObject<wamp_call_element>(json);
                if (callUpdate != null)
                    OnCallEvent?.Invoke(this, callUpdate);
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
