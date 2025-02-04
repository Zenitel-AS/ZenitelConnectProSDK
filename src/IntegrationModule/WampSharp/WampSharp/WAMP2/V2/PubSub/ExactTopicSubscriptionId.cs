#pragma warning disable CS1591
namespace WampSharp.V2.PubSub
{
    public class ExactTopicSubscriptionId : SimpleSubscriptionId
    {
        public ExactTopicSubscriptionId(string topicUri) : base(topicUri)
        {
        }
    }
}