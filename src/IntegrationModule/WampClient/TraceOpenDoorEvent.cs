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
        private readonly object _traceOpenDoorEventGate = new object();

        private TracerOpenDoorEvent tracerOpenDoorEvent;
        private IAsyncDisposable tracerOpenDoorEventDisposable;

        public event EventHandler<wamp_open_door_event> OnWampOpenDoorEvent;

        public async void TraceOpenDoorEvent()
        {
            try
            {
                lock (_traceOpenDoorEventGate)
                {
                    if (tracerOpenDoorEventDisposable != null)
                        return;
                }

                if (_wampRealmProxy == null)
                {
                    OnChildLogString?.Invoke(this, "TraceOpenDoorEvent skipped. WAMP realm proxy is not available.");
                    return;
                }

                IWampTopicProxy topicProxy = _wampRealmProxy.TopicContainer.GetTopicByUri(TraceWampSystemOpenDoor);

                var tracer = new TracerOpenDoorEvent();
                tracer.OnOpenDoorEvent += TracerOpenDoorEvent_OnOpenDoorEvent;
                tracer.OnDebugString += TracerOpenDoorEvent_OnDebugString;

                IAsyncDisposable subscription = await topicProxy
                    .Subscribe(tracer, new SubscribeOptions())
                    .ConfigureAwait(false);

                lock (_traceOpenDoorEventGate)
                {
                    if (tracerOpenDoorEventDisposable != null)
                    {
                        tracer.OnOpenDoorEvent -= TracerOpenDoorEvent_OnOpenDoorEvent;
                        tracer.OnDebugString -= TracerOpenDoorEvent_OnDebugString;
                        subscription.DisposeAsync().AsTask().GetAwaiter().GetResult();
                        return;
                    }

                    tracerOpenDoorEvent = tracer;
                    tracerOpenDoorEventDisposable = subscription;
                }
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception in TraceOpenDoorEvent: " + ex);
            }
        }

        private void TracerOpenDoorEvent_OnDebugString(object sender, string e)
        {
            OnChildLogString?.Invoke(this, "Open Door Subscription Event: " + e);
        }

        private void TracerOpenDoorEvent_OnOpenDoorEvent(object sender, wamp_open_door_event openDoorEvent)
        {
            OnWampOpenDoorEvent?.Invoke(this, openDoorEvent);
        }

        public void TraceOpenDoorEventDispose()
        {
            TracerOpenDoorEvent tracer = null;
            IAsyncDisposable subscription = null;

            lock (_traceOpenDoorEventGate)
            {
                tracer = tracerOpenDoorEvent;
                subscription = tracerOpenDoorEventDisposable;

                tracerOpenDoorEvent = null;
                tracerOpenDoorEventDisposable = null;
            }

            if (tracer != null)
            {
                tracer.OnOpenDoorEvent -= TracerOpenDoorEvent_OnOpenDoorEvent;
                tracer.OnDebugString -= TracerOpenDoorEvent_OnDebugString;
            }

            if (subscription == null)
                return;

            try
            {
                subscription.DisposeAsync().AsTask().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception disposing TraceOpenDoorEvent subscription: " + ex);
            }
        }

        public bool TraceOpenDoorEventIsEnabled()
        {
            lock (_traceOpenDoorEventGate)
            {
                return tracerOpenDoorEventDisposable != null;
            }
        }

        internal class TracerOpenDoorEvent : IWampRawTopicClientSubscriber
        {
            public event EventHandler<wamp_open_door_event> OnOpenDoorEvent;
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

                var openDoorEvent = Newtonsoft.Json.JsonConvert.DeserializeObject<wamp_open_door_event>(json);

                if (openDoorEvent != null)
                    OnOpenDoorEvent?.Invoke(this, openDoorEvent);
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
