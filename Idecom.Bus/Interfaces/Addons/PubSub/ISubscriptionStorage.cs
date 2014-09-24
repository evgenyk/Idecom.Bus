namespace Idecom.Bus.Interfaces.Addons.PubSub
{
    using System;
    using System.Collections.Generic;
    using Addressing;

    public interface ISubscriptionStorage
    {
        IEnumerable<Address> GetSubscribersFor(Type type);
        void Subscribe(Address subscriber, Type type);
        void Unsubscribe(Address subscriber, Type type);
    }
}