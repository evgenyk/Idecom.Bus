using System;
using System.Collections.Generic;
using Idecom.Bus.Addressing;

namespace Idecom.Bus.Interfaces.Addons.PubSub
{
    public interface ISubscriptionStorage
    {
        IEnumerable<Address> GetSubscribersFor<T>() where T : class;
        void Subscribe(Address subscriber, Type type);
        void Unsubscribe(Address subscriber, Type type);
    }
}