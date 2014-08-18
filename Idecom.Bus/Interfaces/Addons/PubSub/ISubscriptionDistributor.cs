namespace Idecom.Bus.Interfaces.Addons.PubSub
{
    using System;
    using System.Collections.Generic;
    using Implementations.UnicastBus;

    public interface ISubscriptionDistributor
    {
        void NotifySubscribersOf(Type messageType, object message, CurrentMessageContext currentMessageContext);
        void SubscribeTo(IEnumerable<Type> events);
        void Unsubscribe(IEnumerable<Type> events);
    }
}