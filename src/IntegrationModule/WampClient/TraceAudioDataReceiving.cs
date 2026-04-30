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
        private readonly object _traceAudioDataReceivingGate = new object();

        private TracerAudioDataReceiving tracerAudioDataReceiving;
        private IAsyncDisposable tracerAudioDataReceivingsDisposable;

        public event EventHandler<wamp_audio_data_receiving> OnAudioDataReceiving;

        public async void TraceAudioDataReceiving()
        {
            try
            {
                lock (_traceAudioDataReceivingGate)
                {
                    if (tracerAudioDataReceivingsDisposable != null)
                        return;
                }

                if (_wampRealmProxy == null)
                {
                    OnChildLogString?.Invoke(this, "TraceAudioDataReceiving skipped. WAMP realm proxy is not available.");
                    return;
                }

                IWampTopicProxy topicProxy = _wampRealmProxy.TopicContainer.GetTopicByUri(TraceWampAudioDataReceiving);

                var tracer = new TracerAudioDataReceiving();
                tracer.OnAudioDataReceiving += TracerAudioDataReceiving_OnAudioDetectionEvent;
                tracer.OnDebugString += TracerAudioDataReceiving_OnDebugString;

                IAsyncDisposable subscription = await topicProxy
                    .Subscribe(tracer, new SubscribeOptions())
                    .ConfigureAwait(false);

                lock (_traceAudioDataReceivingGate)
                {
                    if (tracerAudioDataReceivingsDisposable != null)
                    {
                        tracer.OnAudioDataReceiving -= TracerAudioDataReceiving_OnAudioDetectionEvent;
                        tracer.OnDebugString -= TracerAudioDataReceiving_OnDebugString;
                        subscription.DisposeAsync().AsTask().GetAwaiter().GetResult();
                        return;
                    }

                    tracerAudioDataReceiving = tracer;
                    tracerAudioDataReceivingsDisposable = subscription;
                }
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception in TraceAudioDataReceiving: " + ex);
            }
        }

        private void TracerAudioDataReceiving_OnDebugString(object sender, string e)
        {
            OnChildLogString?.Invoke(this, "Audio Data Receiving Subscription: " + e);
        }

        private void TracerAudioDataReceiving_OnAudioDetectionEvent(object sender, wamp_audio_data_receiving audioEvent)
        {
            OnAudioDataReceiving?.Invoke(this, audioEvent);
        }

        public void TraceAudioDataReceivingDispose()
        {
            TracerAudioDataReceiving tracer = null;
            IAsyncDisposable subscription = null;

            lock (_traceAudioDataReceivingGate)
            {
                tracer = tracerAudioDataReceiving;
                subscription = tracerAudioDataReceivingsDisposable;

                tracerAudioDataReceiving = null;
                tracerAudioDataReceivingsDisposable = null;
            }

            if (tracer != null)
            {
                tracer.OnAudioDataReceiving -= TracerAudioDataReceiving_OnAudioDetectionEvent;
                tracer.OnDebugString -= TracerAudioDataReceiving_OnDebugString;
            }

            if (subscription == null)
                return;

            try
            {
                subscription.DisposeAsync().AsTask().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception disposing TraceAudioDataReceiving subscription: " + ex);
            }
        }

        public bool TraceAudioDataReceivingIsEnabled()
        {
            lock (_traceAudioDataReceivingGate)
            {
                return tracerAudioDataReceivingsDisposable != null;
            }
        }

        internal class TracerAudioDataReceiving : IWampRawTopicClientSubscriber
        {
            public event EventHandler<wamp_audio_data_receiving> OnAudioDataReceiving;
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

                var audioEvent = Newtonsoft.Json.JsonConvert.DeserializeObject<wamp_audio_data_receiving>(json);

                if (audioEvent != null)
                    OnAudioDataReceiving?.Invoke(this, audioEvent);
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
