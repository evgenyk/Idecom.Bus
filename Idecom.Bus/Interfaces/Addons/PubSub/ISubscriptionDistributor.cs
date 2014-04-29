using System;
using System.Collections.Generic;

namespace Idecom.Bus.Interfaces.Addons.PubSub
{
    public interface ISubscriptionDistributor
    {
        void NotifySubscribers(object message);
        void SubscribeTo(IEnumerable<Type> events);
    }
}