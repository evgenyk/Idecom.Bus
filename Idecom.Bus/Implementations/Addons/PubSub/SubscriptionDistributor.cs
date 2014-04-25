using Idecom.Bus.Interfaces.Addons.PubSub;

namespace Idecom.Bus.Implementations.Addons.PubSub
{
    internal class SubscriptionDistributor: ISubscriptionDistributor
    {
        public ISubscriptionStorage Storage { get; set; }

        public void NotifySubscribers(object message)
        {
            var subscribers = Storage.GetSubscribersFor(message);
        }
    }
}