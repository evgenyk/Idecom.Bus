using System;
using System.Collections.Generic;
using Idecom.Bus.Addressing;

namespace Idecom.Bus.Interfaces.Addons.PubSub
{
    public interface ISubscriptionStorage
    {
        IEnumerable<Address> GetSubscribersFor(Type eventType);
        void Subscribe(Address subscriber, Type eventType);
        void Unsubscribe(Address subscriber, Type eventType);
    }
}