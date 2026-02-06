using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using WampSharp.Core.Serialization;
using WampSharp.V2.Client;
using WampSharp.V2.Core.Contracts;
using WampSharp.V2.PubSub;

namespace Wamp.Client
{
    public partial class WampClient
    {
        /***********************************************************************************************************************/
        /********************                    Trace Device GPO Status Event                               *******************/
        /***********************************************************************************************************************/

        // Gate for subscribe/unsubscribe maps.
        private readonly object _gpoTraceGate = new object();

        // Per-device tracer instances (used to preserve dirno for EventEx routing).
        private readonly Dictionary<string, TracerDeviceGPOStatusEvent> _gpoTracers =
            new Dictionary<string, TracerDeviceGPOStatusEvent>(StringComparer.Ordinal);

        // Per-device subscription handles returned from WAMP Subscribe.
        private readonly Dictionary<string, IAsyncDisposable> _gpoSubscriptions =
            new Dictionary<string, IAsyncDisposable>(StringComparer.Ordinal);

        /// <summary>
        /// Legacy event: emits GPO change payload only.
        /// NOTE: The payload does NOT include the device dirno, therefore consumers cannot reliably route it.
        /// Prefer <see cref="OnWampDeviceGPOStatusEventEx"/>.
        /// </summary>
        public event EventHandler<wamp_device_gpio_element> OnWampDeviceGPOStatusEvent;

        /// <summary>
        /// Preferred event: emits both the device dirno and the raw payload element.
        /// This is required because trace payloads do not include dirno.
        /// </summary>
        public event EventHandler<WampGpioEventArgs> OnWampDeviceGPOStatusEventEx;

        /***********************************************************************************************************************/
        internal sealed class TraceDeviceGPOOptions : SubscribeOptions
        /***********************************************************************************************************************/
        {
            // If a data member is not set, it will not be sent to WAAPI.
            [DataMember(Name = "dirno")]
            public string dirno { get; set; }
        }

        /// <summary>
        /// Enables the subscription of GPO (General Purpose Output) status changes for all devices.
        /// </summary>
        public async void TraceDeviceGPOStatusEvent()
        {
            try
            {
                // Fast path: already subscribed
                lock (_gpoTraceGate)
                {
                    if (_gpoSubscriptions.Count > 0)
                        return;
                }

                var options = new TraceDeviceGPOOptions();
                string uri = "com.zenitel.device.gpo";

                OnChildLogString?.Invoke(this, "TraceDeviceGPOStatusEvent - uri: " + uri);

                IWampTopicProxy topicProxy = _wampRealmProxy.TopicContainer.GetTopicByUri(uri);

                // Create tracer for EventEx routing.
                var tracer = new TracerDeviceGPOStatusEvent(null);
                tracer.OnDeviceGPOStatusEvent += TracerDeviceGPOStatusEvent_OnDeviceGPOStatusEvent;
                tracer.OnDebugString += TracerDeviceGPOStatusEvent_OnDebugString;

                var subscription = await topicProxy.Subscribe(tracer, options).ConfigureAwait(false);

                // Commit subscription under gate, handle race (double-subscribe).
                lock (_gpoTraceGate)
                {
                    if (_gpoSubscriptions.Count > 0)
                    {
                        // Someone subscribed concurrently; discard this subscription.
                        subscription.DisposeAsync();
                        return;
                    }

                    _gpoTracers["_global"] = tracer;
                    _gpoSubscriptions["_global"] = subscription;
                }
            }
            catch (WampException wex) when (wex.ErrorUri == "wamp.error.not_authorized")
            {
                OnChildLogString?.Invoke(this, "TraceDeviceGPOStatusEvent - Not authorized to subscribe to GPO events.");
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "TraceDeviceGPOStatusEvent - Exception: " + ex.Message);
            }
        }

        private void TracerDeviceGPOStatusEvent_OnDebugString(object sender, string e)
        {
            OnChildLogString?.Invoke(this, "DeviceGPO Status Event: " + e);
        }

        private void TracerDeviceGPOStatusEvent_OnDeviceGPOStatusEvent(object sender, wamp_device_gpio_element gpioElement)
        {
            if (gpioElement == null)
                return;

            // Legacy event (no dirno).
            OnWampDeviceGPOStatusEvent?.Invoke(this, gpioElement);

            // Preferred event (dirno + payload).
            // Use tracer.Dirno for per-device subscriptions, fall back to
            // the dirno embedded in the JSON payload for global subscriptions.
            var tracer = sender as TracerDeviceGPOStatusEvent;
            var dirno = tracer?.Dirno;
            if (string.IsNullOrEmpty(dirno))
                dirno = gpioElement.dirno;

            if (!string.IsNullOrEmpty(dirno))
            {
                OnWampDeviceGPOStatusEventEx?.Invoke(this, new WampGpioEventArgs(dirno, gpioElement));
            }
        }

        /// <summary>
        /// Terminates the subscription of GPO status updates for a specific device.
        /// </summary>
        public void TraceDeviceGPOStatusEventDispose(string dirNo)
        {
            if (string.IsNullOrEmpty(dirNo))
                return;

            IAsyncDisposable subscription = null;

            lock (_gpoTraceGate)
            {
                if (_gpoSubscriptions.TryGetValue(dirNo, out subscription))
                {
                    _gpoSubscriptions.Remove(dirNo);
                    _gpoTracers.Remove(dirNo);
                }
            }

            if (subscription != null)
            {
                try
                {
                    subscription.DisposeAsync();
                }
                catch (Exception ex)
                {
                    OnChildLogString?.Invoke(this, "Exception disposing GPO subscription: " + ex);
                }
            }
        }

        /// <summary>
        /// Terminates ALL active GPO subscriptions (backward-compatible with your previous global Dispose).
        /// </summary>
        public void TraceDeviceGPOStatusEventDispose()
        {
            List<IAsyncDisposable> subscriptions;

            lock (_gpoTraceGate)
            {
                subscriptions = new List<IAsyncDisposable>(_gpoSubscriptions.Values);
                _gpoSubscriptions.Clear();
                _gpoTracers.Clear();
            }

            foreach (var sub in subscriptions)
            {
                try
                {
                    sub.DisposeAsync();
                }
                catch (Exception ex)
                {
                    OnChildLogString?.Invoke(this, "Exception disposing GPO subscription: " + ex);
                }
            }
        }

        /// <summary>
        /// Returns true if there is at least one active GPO subscription.
        /// </summary>
        public bool TraceDeviceGPOStatusEventIsEnabled()
        {
            lock (_gpoTraceGate)
            {
                return _gpoSubscriptions.Count > 0;
            }
        }

        /// <summary>
        /// Returns true if the specified device has an active GPO subscription.
        /// </summary>
        public bool TraceDeviceGPOStatusEventIsEnabled(string dirNo)
        {
            if (string.IsNullOrEmpty(dirNo))
                return false;

            lock (_gpoTraceGate)
            {
                return _gpoSubscriptions.ContainsKey(dirNo);
            }
        }

        /***********************************************************************************************************************/
        internal sealed class TracerDeviceGPOStatusEvent : IWampRawTopicClientSubscriber
        /***********************************************************************************************************************/
        {
            public string Dirno { get; private set; }

            public event EventHandler<wamp_device_gpio_element> OnDeviceGPOStatusEvent;
            public event EventHandler<string> OnDebugString;

            public TracerDeviceGPOStatusEvent(string dirno)
            {
                Dirno = dirno;
            }

            public void Event<TMessage>(IWampFormatter<TMessage> formatter, long publicationId, EventDetails details)
            {
                string txt = "Got event with publication id: " + publicationId;
                OnDebugString?.Invoke(this, txt);
            }

            public void Event<TMessage>(IWampFormatter<TMessage> formatter, long publicationId, EventDetails details, TMessage[] arguments)
            {
                if (arguments == null || arguments.Length == 0 || arguments[0] == null)
                    return;

                string json_str = arguments[0].ToString();
                OnDebugString?.Invoke(this, json_str);

                var gpioElement = Newtonsoft.Json.JsonConvert.DeserializeObject<wamp_device_gpio_element>(json_str);
                OnDeviceGPOStatusEvent?.Invoke(this, gpioElement);
            }

            public void Event<TMessage>(
                IWampFormatter<TMessage> formatter,
                long publicationId,
                EventDetails details,
                TMessage[] arguments,
                IDictionary<string, TMessage> argumentsKeywords)
            {
                string txt = "Event with argumentsKeywords is not supported.";
                OnDebugString?.Invoke(this, txt);
            }
        }
    }
}
