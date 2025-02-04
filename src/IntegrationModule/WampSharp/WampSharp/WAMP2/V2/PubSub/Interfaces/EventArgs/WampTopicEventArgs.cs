#pragma warning disable CS1591
using System;

namespace WampSharp.V2.PubSub
{
    public class WampTopicEventArgs : EventArgs
    {
        public WampTopicEventArgs(IWampTopic topic)
        {
            Topic = topic;
        }

        /// <summary>
        /// Gets the relevant topic.
        /// </summary>
        public IWampTopic Topic { get; }
    }
}