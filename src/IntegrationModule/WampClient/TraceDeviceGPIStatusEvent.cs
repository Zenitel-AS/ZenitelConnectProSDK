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
                const string key = "_global";

                lock (_gpiTraceGate)
                {
                    if (_gpiSubscriptions.ContainsKey(key))
                        return;
                }

                if (_wampRealmProxy == null)
                {
                    OnChildLogString?.Invoke(this, "TraceDeviceGPIStatusEvent skipped. WAMP realm proxy is not available.");
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

                lock (_gpiTraceGate)
                {
                    if (_gpiSubscriptions.ContainsKey(key))
                    {
                        tracer.OnDeviceGPIStatusEvent -= TracerDeviceGPIStatusEvent_OnDeviceGPIStatusEvent;
                        tracer.OnDebugString -= TracerDeviceGPIStatusEvent_OnDebugString;
                        subscription.DisposeAsync().AsTask().GetAwaiter().GetResult();
                        return;
                    }

                    _gpiTracers[key] = tracer;
                    _gpiSubscriptions[key] = subscription;
                }
            }
            catch (WampException wex) when (wex.ErrorUri == "wamp.error.not_authorized")
            {
                OnChildLogString?.Invoke(this, "TraceDeviceGPIStatusEvent - Not authorized to subscribe to GPI events.");
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "TraceDeviceGPIStatusEvent - Exception: " + ex);
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

            OnWampDeviceGPIStatusEvent?.Invoke(this, gpioElement);

            var tracer = sender as TracerDeviceGPIStatusEvent;
            var dirno = tracer?.Dirno;

            if (string.IsNullOrEmpty(dirno))
                dirno = gpioElement.dirno;

            if (!string.IsNullOrEmpty(dirno))
                OnWampDeviceGPIStatusEventEx?.Invoke(this, new WampGpioEventArgs(dirno, gpioElement));
        }

        /// <summary>
        /// Terminates the subscription of GPI status updates for a specific device.
        /// </summary>
        public void TraceDeviceGPIStatusEventDispose(string dirNo)
        {
            if (string.IsNullOrEmpty(dirNo))
                return;

            DisposeGpiSubscription(dirNo);
        }

        /// <summary>
        /// Terminates ALL active GPI subscriptions (backward-compatible with your previous global Dispose).
        /// </summary>
        public void TraceDeviceGPIStatusEventDispose()
        {
            List<string> keys;

            lock (_gpiTraceGate)
            {
                keys = new List<string>(_gpiSubscriptions.Keys);
            }

            foreach (string key in keys)
                DisposeGpiSubscription(key);
        }

        private void DisposeGpiSubscription(string key)
        {
            TracerDeviceGPIStatusEvent tracer = null;
            IAsyncDisposable subscription = null;

            lock (_gpiTraceGate)
            {
                _gpiTracers.TryGetValue(key, out tracer);
                _gpiSubscriptions.TryGetValue(key, out subscription);

                _gpiTracers.Remove(key);
                _gpiSubscriptions.Remove(key);
            }

            if (tracer != null)
            {
                tracer.OnDeviceGPIStatusEvent -= TracerDeviceGPIStatusEvent_OnDeviceGPIStatusEvent;
                tracer.OnDebugString -= TracerDeviceGPIStatusEvent_OnDebugString;
            }

            if (subscription == null)
                return;

            try
            {
                subscription.DisposeAsync().AsTask().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                OnChildLogString?.Invoke(this, "Exception disposing GPI subscription: " + ex);
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

                string json = arguments[0].ToString();
                OnDebugString?.Invoke(this, json);

                var gpioElement = Newtonsoft.Json.JsonConvert.DeserializeObject<wamp_device_gpio_element>(json);

                if (gpioElement != null)
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
