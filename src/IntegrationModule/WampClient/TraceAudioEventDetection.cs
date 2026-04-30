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
        private readonly object _traceAudioEventDetectionGate = new object();

        private TracerAudioEventDetection tracerAudioEventDetection;
        private IAsyncDisposable tracerAudioEventDetectionsDisposable;

        public event EventHandler<wamp_audio_event_detection> OnAudioEventDetection;

        public async void TraceAudioEventDetection()
        {
            try
            {
                lock (_traceAudioEventDetectionGate)
                {
                    if (tracerAudioEventDetectionsDisposable != null)
                        return;
                }

                if (_wampRealmProxy == null)
                {
                    OnChildLogString?.Invoke(this, "TraceAudioEventDetection skipped. WAMP realm proxy is not available.");
                    return;
                }

                IWampTopicProxy topicProxy = _wampRealmProxy.TopicContainer.GetTopicByUri(TraceWampAudioEvents);

                var tracer = new TracerAudioEventDetection();
                tracer.OnAudioEventDetection += TracerAudioEventDetection_OnAudioDetectionEvent;
                tracer.OnDebugString += TracerAudioEventDetection_OnDebugString;

                IAsyncDisposable subscription = await topicProxy
                    .Subscribe(tracer, new SubscribeOptions())
                    .ConfigureAwait(false);

                lock (_traceAudioEventDetectionGate)
                {
                    if (tracerAudioEventDetectionsDisposable != null)
                    {
                        tracer.OnAudioEventDetection -= TracerAudioEventDetection_OnAudioDetectionEvent;
                        tracer.OnDebugString -= TracerAudioEventDetection_OnDebugString;
                        subscription.DisposeAsync().AsTask().GetAwaiter().GetResult();
                        return;
                    }

                    tracerAudioEventDetection = tracer;
                    tracerAudioEventDetectionsDisposable = subscription;
                }
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception in TraceAudioEventDetection: " + ex);
            }
        }

        private void TracerAudioEventDetection_OnDebugString(object sender, string e)
        {
            OnChildLogString?.Invoke(this, "Audio Event Detection Subscription: " + e);
        }

        private void TracerAudioEventDetection_OnAudioDetectionEvent(object sender, wamp_audio_event_detection audioEvent)
        {
            OnAudioEventDetection?.Invoke(this, audioEvent);
        }

        public void TraceAudioEventDetectionDispose()
        {
            TracerAudioEventDetection tracer = null;
            IAsyncDisposable subscription = null;

            lock (_traceAudioEventDetectionGate)
            {
                tracer = tracerAudioEventDetection;
                subscription = tracerAudioEventDetectionsDisposable;

                tracerAudioEventDetection = null;
                tracerAudioEventDetectionsDisposable = null;
            }

            if (tracer != null)
            {
                tracer.OnAudioEventDetection -= TracerAudioEventDetection_OnAudioDetectionEvent;
                tracer.OnDebugString -= TracerAudioEventDetection_OnDebugString;
            }

            if (subscription == null)
                return;

            try
            {
                subscription.DisposeAsync().AsTask().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception disposing TraceAudioEventDetection subscription: " + ex);
            }
        }

        public bool TraceAudioEventDetectionIsEnabled()
        {
            lock (_traceAudioEventDetectionGate)
            {
                return tracerAudioEventDetectionsDisposable != null;
            }
        }

        internal class TracerAudioEventDetection : IWampRawTopicClientSubscriber
        {
            public event EventHandler<wamp_audio_event_detection> OnAudioEventDetection;
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

                var audioEvent = Newtonsoft.Json.JsonConvert.DeserializeObject<wamp_audio_event_detection>(json);

                if (audioEvent != null)
                    OnAudioEventDetection?.Invoke(this, audioEvent);
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
