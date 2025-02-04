#pragma warning disable CS1591
using System;

namespace WampSharp.V2.PubSub
{
    public class WampSubscriptionRemoveEventArgs : EventArgs
    {
        private readonly long mSubscriptionId;

        public WampSubscriptionRemoveEventArgs(long session, long subscriptionId)
        {
            Session = session;
            mSubscriptionId = subscriptionId;
        }

        public long Session { get; }

        public long SubscriptionId => mSubscriptionId;
    }
}