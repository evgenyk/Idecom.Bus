using System;
using System.Collections.Generic;
using Idecom.Bus.Implementations.UnicastBus;

namespace Idecom.Bus.Interfaces.Addons.PubSub
{
    public interface ISubscriptionDistributor
    {
        void NotifySubscribersOf<T>(object message, CurrentMessageContext currentMessageContext) where T : class;
        void SubscribeTo(IEnumerable<Type> events);
        void Unsubscribe(IEnumerable<Type> events);
    }
}