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
        /********************            Trace Device GPI (General Purpose Input) Status Event               *******************/
        /***********************************************************************************************************************/

        // Gate for subscribe/unsubscribe maps.
        private readonly object _gpiTraceGate = new object();

        // Per-device tracer instances (mostly to preserve dirno for EventEx routing).
        private readonly Dictionary<string, TracerDeviceGPIStatusEvent> _gpiTracers =
            new Dictionary<string, TracerDeviceGPIStatusEvent>(StringComparer.Ordinal);

        // Per-device subscription handles returned from WAMP Subscribe.
        private readonly Dictionary<string, IAsyncDisposable> _gpiSubscriptions =
            new Dictionary<string, IAsyncDisposable>(StringComparer.Ordinal);

        /// <summary>
        /// Legacy event: emits GPI change payload only.
        /// NOTE: The payload does NOT include the device dirno, therefore consumers cannot reliably route it.
        /// Prefer <see cref="OnWampDeviceGPIStatusEventEx"/>.
        /// </summary>
        public event EventHandler<wamp_device_gpio_element> OnWampDeviceGPIStatusEvent;

        /// <summary>
        /// Preferred event: emits both the device dirno and the raw payload element.
        /// This is required because trace payloads do not include dirno.
        /// </summary>
        public event EventHandler<WampGpioEventArgs> OnWampDeviceGPIStatusEventEx;

        /***********************************************************************************************************************/
        internal sealed class TraceDeviceGPIOptions : SubscribeOptions
        /***********************************************************************************************************************/
        {
            // If a data member is not set, it will not be sent to WAAPI.
            [DataMember(Name = "dirno")]
            public string dirno { get; set; }
        }

        /// <summary>
        /// Enables the subscription of GPI (General Purpose Input) status changes for all devices.
        /// </summary>
        public async void TraceDeviceGPIStatusEvent()
        {
            try
            {
                // Fast path: already subscribed
                lock (_gpiTraceGate)
                {
                    if (_gpiSubscriptions.Count > 0)
                        return;
                }

                var options = new TraceDeviceGPIOptions();
                string uri = "com.zenitel.device.gpi";

                OnChildLogString?.Invoke(this, "TraceDeviceGPIStatusEvent - uri: " + uri);

                IWampTopicProxy topicProxy = _wampRealmProxy.TopicContainer.GetTopicByUri(uri);

                // Create tracer for EventEx routing.
                var tracer = new TracerDeviceGPIStatusEvent(null);
                tracer.OnDeviceGPIStatusEvent += TracerDeviceGPIStatusEvent_OnDeviceGPIStatusEvent;
                tracer.OnDebugString += TracerDeviceGPIStatusEvent_OnDebugString;

                var subscription = await topicProxy.Subscribe(tracer, options).ConfigureAwait(false);

                // Commit subscription under gate, handle race (double-subscribe).
                lock (_gpiTraceGate)
                {
                    if (_gpiSubscriptions.Count > 0)
                    {
                        // Someone subscribed concurrently; discard this subscription.
                        subscription.DisposeAsync();
                        return;
                    }

                    _gpiTracers["_global"] = tracer;
                    _gpiSubscriptions["_global"] = subscription;
                }
            }
            catch (WampException wex) when (wex.ErrorUri == "wamp.error.not_authorized")
            {
                OnChildLogString?.Invoke(this, "TraceDeviceGPIStatusEvent - Not authorized to subscribe to GPI events.");
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "TraceDeviceGPIStatusEvent - Exception: " + ex.Message);
            }
        }

        private void TracerDeviceGPIStatusEvent_OnDebugString(object sender, string e)
        {
            OnChildLogString?.Invoke(this, "DeviceGPI Status Event: " + e);
        }

        private void TracerDeviceGPIStatusEvent_OnDeviceGPIStatusEvent(object sender, wamp_device_gpio_element gpioElement)
        {
            if (gpioElement == null)
                return;

            OnChildLogString?.Invoke(this, "DeviceGPI Status Event: " + gpioElement);

            // Legacy event (no dirno).
            OnWampDeviceGPIStatusEvent?.Invoke(this, gpioElement);

            // Preferred event (dirno + payload).
            // Use tracer.Dirno for per-device subscriptions, fall back to
            // the dirno embedded in the JSON payload for global subscriptions.
            var tracer = sender as TracerDeviceGPIStatusEvent;
            var dirno = tracer?.Dirno;
            if (string.IsNullOrEmpty(dirno))
                dirno = gpioElement.dirno;

            if (!string.IsNullOrEmpty(dirno))
            {
                OnWampDeviceGPIStatusEventEx?.Invoke(this, new WampGpioEventArgs(dirno, gpioElement));
            }
        }

        /// <summary>
        /// Terminates the subscription of GPI status updates for a specific device.
        /// </summary>
        public void TraceDeviceGPIStatusEventDispose(string dirNo)
        {
            if (string.IsNullOrEmpty(dirNo))
                return;

            IAsyncDisposable subscription = null;

            lock (_gpiTraceGate)
            {
                if (_gpiSubscriptions.TryGetValue(dirNo, out subscription))
                {
                    _gpiSubscriptions.Remove(dirNo);
                    _gpiTracers.Remove(dirNo);
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
                    OnChildLogString?.Invoke(this, "Exception disposing GPI subscription: " + ex);
                }
            }
        }

        /// <summary>
        /// Terminates ALL active GPI subscriptions (backward-compatible with your previous global Dispose).
        /// </summary>
        public void TraceDeviceGPIStatusEventDispose()
        {
            List<IAsyncDisposable> subscriptions;

            lock (_gpiTraceGate)
            {
                subscriptions = new List<IAsyncDisposable>(_gpiSubscriptions.Values);
                _gpiSubscriptions.Clear();
                _gpiTracers.Clear();
            }

            foreach (var sub in subscriptions)
            {
                try
                {
                    sub.DisposeAsync();
                }
                catch (Exception ex)
                {
                    OnChildLogString?.Invoke(this, "Exception disposing GPI subscription: " + ex);
                }
            }
        }

        /// <summary>
        /// Returns true if there is at least one active GPI subscription.
        /// </summary>
        public bool TraceDeviceGPIStatusEventIsEnabled()
        {
            lock (_gpiTraceGate)
            {
                return _gpiSubscriptions.Count > 0;
            }
        }

        /// <summary>
        /// Returns true if the specified device has an active GPI subscription.
        /// </summary>
        public bool TraceDeviceGPIStatusEventIsEnabled(string dirNo)
        {
            if (string.IsNullOrEmpty(dirNo))
                return false;

            lock (_gpiTraceGate)
            {
                return _gpiSubscriptions.ContainsKey(dirNo);
            }
        }

        /***********************************************************************************************************************/
        internal sealed class TracerDeviceGPIStatusEvent : IWampRawTopicClientSubscriber
        /***********************************************************************************************************************/
        {
            public string Dirno { get; private set; }

            public event EventHandler<wamp_device_gpio_element> OnDeviceGPIStatusEvent;
            public event EventHandler<string> OnDebugString;

            public TracerDeviceGPIStatusEvent(string dirno)
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
                OnDeviceGPIStatusEvent?.Invoke(this, gpioElement);
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
