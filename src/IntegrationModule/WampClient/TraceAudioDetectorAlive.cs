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
        private readonly object _traceAudioDetectorAliveGate = new object();

        private TracerAudioDetectorAlive tracerAudioDetectorAlive;
        private IAsyncDisposable tracerAudioDetectorAlivesDisposable;

        /// <summary>
        /// Occurs when the audio detector publishes a heartbeat indicating that it is alive.
        /// </summary>
        public event EventHandler<wamp_audio_detector_alive> OnAudioDetectorAlive;

        /// <summary>
        /// Subscribes to audio detector alive notifications from the active WAMP realm.
        /// </summary>
        public async void TraceAudioDetectorAlive()
        {
            try
            {
                lock (_traceAudioDetectorAliveGate)
                {
                    if (tracerAudioDetectorAlivesDisposable != null)
                        return;
                }

                if (_wampRealmProxy == null)
                {
                    OnChildLogString?.Invoke(this, "TraceAudioDetectorAlive skipped. WAMP realm proxy is not available.");
                    return;
                }

                IWampTopicProxy topicProxy = _wampRealmProxy.TopicContainer.GetTopicByUri(TraceWampAudioDetectorAlive);

                var tracer = new TracerAudioDetectorAlive();
                tracer.OnAudioDetectorAlive += TracerAudioDetectorAlive_OnAudioDetectionEvent;
                tracer.OnDebugString += TracerAudioDetectorAlive_OnDebugString;

                IAsyncDisposable subscription = await topicProxy
                    .Subscribe(tracer, new SubscribeOptions())
                    .ConfigureAwait(false);

                lock (_traceAudioDetectorAliveGate)
                {
                    if (tracerAudioDetectorAlivesDisposable != null)
                    {
                        tracer.OnAudioDetectorAlive -= TracerAudioDetectorAlive_OnAudioDetectionEvent;
                        tracer.OnDebugString -= TracerAudioDetectorAlive_OnDebugString;
                        subscription.DisposeAsync().AsTask().GetAwaiter().GetResult();
                        return;
                    }

                    tracerAudioDetectorAlive = tracer;
                    tracerAudioDetectorAlivesDisposable = subscription;
                }
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception in TraceAudioDetectorAlive: " + ex);
            }
        }

        private void TracerAudioDetectorAlive_OnDebugString(object sender, string e)
        {
            OnChildLogString?.Invoke(this, "Audio Detector Alive Subscription: " + e);
        }

        private void TracerAudioDetectorAlive_OnAudioDetectionEvent(object sender, wamp_audio_detector_alive audioEvent)
        {
            OnAudioDetectorAlive?.Invoke(this, audioEvent);
        }

        /// <summary>
        /// Disposes the audio detector alive subscription and detaches the internal tracer handlers.
        /// </summary>
        public void TraceAudioDetectorAliveDispose()
        {
            TracerAudioDetectorAlive tracer = null;
            IAsyncDisposable subscription = null;

            lock (_traceAudioDetectorAliveGate)
            {
                tracer = tracerAudioDetectorAlive;
                subscription = tracerAudioDetectorAlivesDisposable;

                tracerAudioDetectorAlive = null;
                tracerAudioDetectorAlivesDisposable = null;
            }

            if (tracer != null)
            {
                tracer.OnAudioDetectorAlive -= TracerAudioDetectorAlive_OnAudioDetectionEvent;
                tracer.OnDebugString -= TracerAudioDetectorAlive_OnDebugString;
            }

            if (subscription == null)
                return;

            try
            {
                subscription.DisposeAsync().AsTask().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception disposing TraceAudioDetectorAlive subscription: " + ex);
            }
        }

        /// <summary>
        /// Determines whether the audio detector alive subscription is currently active.
        /// </summary>
        /// <returns><see langword="true"/> when the subscription is active; otherwise, <see langword="false"/>.</returns>
        public bool TraceAudioDetectorAliveIsEnabled()
        {
            lock (_traceAudioDetectorAliveGate)
            {
                return tracerAudioDetectorAlivesDisposable != null;
            }
        }

        internal class TracerAudioDetectorAlive : IWampRawTopicClientSubscriber
        {
            public event EventHandler<wamp_audio_detector_alive> OnAudioDetectorAlive;
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

                var audioEvent = Newtonsoft.Json.JsonConvert.DeserializeObject<wamp_audio_detector_alive>(json);

                if (audioEvent != null)
                    OnAudioDetectorAlive?.Invoke(this, audioEvent);
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
