using System;
using System.Collections.Generic;
using Idecom.Bus.Addressing;
using Idecom.Bus.Interfaces.Addons.PubSub;

namespace Idecom.Bus.Implementations.Addons.PubSub
{
    internal class SubscriptionDistributor: ISubscriptionDistributor
    {
        public ISubscriptionStorage Storage { get; set; }
        public Address LocalAddress { get; set; }

        public void NotifySubscribers(object message)
        {
            var subscribers = Storage.GetSubscribersFor(message);
        }

        public void SubscribeTo(IEnumerable<Type> events)
        {
            foreach (var @event in events)
            {
                Storage.Subscribe(Address, );
            }
        }
    }
}