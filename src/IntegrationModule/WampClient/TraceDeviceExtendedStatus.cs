using System;
using System.Collections.Generic;
using WampSharp.Core.Serialization;
using WampSharp.V2.Client;
using WampSharp.V2.PubSub;
using WampSharp.V2.Core.Contracts;


namespace Wamp.Client
{

    public partial class WampClient
    {
        /***********************************************************************************************************************/
        /********************                 Device Extended Status Event Subscription                         *******************/
        /***********************************************************************************************************************/

        TracerDeviceExtendedStatusEvent tracerDeviceExtendedStatusEvent = null;
        IAsyncDisposable tracerDeviceExtendedStatusEventDisposable = null;

        /// <summary>
        /// If set (not null) Device Registration changes will be sent to event handler  OnWampDeviceRegistrationEven
        /// </summary>
        public event EventHandler<wamp_device_extended_status> OnWampDeviceExtendedStatusEvent;


        /// <summary>
        /// This method enables the subscription of Device Registration Status Changes.
        /// </summary>
        /***********************************************************************************************************************/
        public async void TraceDeviceExtendedStatusEvent()
        /***********************************************************************************************************************/
        {
            IWampTopicProxy topicProxy = _wampRealmProxy.TopicContainer.GetTopicByUri(TraceDeviceExtendedStatus);

            tracerDeviceExtendedStatusEvent = new TracerDeviceExtendedStatusEvent();
            tracerDeviceExtendedStatusEvent.OnDeviceExtendedStatusEvent += TracerDeviceExtendedStatusEvent_OnRegistrationEvent;
            tracerDeviceExtendedStatusEvent.OnDebugString += TracerDeviceRegistrationEvent_OnDebugString;

            tracerDeviceExtendedStatusEventDisposable = await topicProxy.Subscribe(tracerDeviceExtendedStatusEvent, new SubscribeOptions()).ConfigureAwait(false);
        }


        /***********************************************************************************************************************/
        private void TracerDeviceExtendedStatusEvent_OnDebugString(object sender, string e)
        /***********************************************************************************************************************/
        {
            OnChildLogString?.Invoke(this, "Device Extended Status Event: " + e);
        }


        /***********************************************************************************************************************/
        private void TracerDeviceExtendedStatusEvent_OnRegistrationEvent(object sender, wamp_device_extended_status deviceStat)
        /***********************************************************************************************************************/
        {
            OnWampDeviceExtendedStatusEvent?.Invoke(this, deviceStat);
        }

        /// <summary>This method terminates the subscription of Device Registration Status Updates.</summary>
        /***********************************************************************************************************************/
        public void TraceDeviceExtendedStatusEventDispose()
        /***********************************************************************************************************************/
        {
            if (tracerDeviceExtendedStatusEventDisposable != null)
            {
                tracerDeviceExtendedStatusEventDisposable.DisposeAsync();
                tracerDeviceExtendedStatusEventDisposable = null;
                tracerDeviceExtendedStatusEvent = null;
            }
        }


        /// <summary>This method returns the status of Device Registration changes subscription.</summary>
        /// <returns>DEvice Registration Status change subscription enabled/disabled as true/false.</returns>
        /***********************************************************************************************************************/
        public bool TraceDeviceExtendedStatusIsEnabled()
        /***********************************************************************************************************************/
        {
            if (tracerDeviceExtendedStatusEvent == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        /***********************************************************************************************************************/
        internal class TracerDeviceExtendedStatusEvent : IWampRawTopicClientSubscriber
        /***********************************************************************************************************************/
        {

            public event EventHandler<wamp_device_extended_status> OnDeviceExtendedStatusEvent;
            public event EventHandler<string> OnDebugString;

            /***********************************************************************************************************************/
            public void Event<TMessage>(IWampFormatter<TMessage> formatter, long publicationId, EventDetails details)
            /***********************************************************************************************************************/
            {
                OnDebugString?.Invoke(this, "Got event with publication id: " + publicationId);
            }


            /***********************************************************************************************************************/
            public void Event<TMessage>(IWampFormatter<TMessage> formatter, long publicationId, EventDetails details, TMessage[] arguments)
            /***********************************************************************************************************************/
            {
                string json_str = arguments[0].ToString();
                OnDebugString?.Invoke(this, json_str);

                wamp_device_extended_status regUpdate = Newtonsoft.Json.JsonConvert.DeserializeObject<wamp_device_extended_status>(arguments[0].ToString());
                OnDeviceExtendedStatusEvent?.Invoke(this, regUpdate);
            }


            /***********************************************************************************************************************/
            public void Event<TMessage>(IWampFormatter<TMessage> formatter, long publicationId, EventDetails details, TMessage[] arguments,
                                        IDictionary<string, TMessage> argumentsKeywords)
            /***********************************************************************************************************************/
            {
                OnDebugString?.Invoke(this, "Event<TMessage>(IWampFormatter<TMessage> formatter, long publicationId, EventDetails details, TMessage[] arguments, IDictionary<string, TMessage> argumentsKeywords) IS NOT SUPPORTED");
            }

        }
    }
}
