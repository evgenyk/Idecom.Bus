namespace Idecom.Bus.Interfaces.Addons.PubSub
{
    public interface ISubscriptionDistributor
    {
        void NotifySubscribers(object message);
    }
}