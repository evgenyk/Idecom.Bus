namespace Idecom.Bus.Interfaces.Addons.PubSub
{
    using System;
    using System.Collections.Generic;
    using Implementations.UnicastBus;

    public interface ISubscriptionDistributor
    {
        void NotifySubscribersOf<T>(object message, CurrentMessageContext currentMessageContext) where T : class;
        void SubscribeTo(IEnumerable<Type> events);
    }
}