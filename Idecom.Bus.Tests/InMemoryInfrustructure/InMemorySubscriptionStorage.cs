using System;
using System.Collections.Generic;
using Idecom.Bus.Addressing;
using Idecom.Bus.Interfaces.Addons.PubSub;

namespace Idecom.Bus.Tests.InMemoryInfrustructure
{
    public class InMemorySubscriptionStorage : ISubscriptionStorage
    {
        public IEnumerable<Address> GetSubscribersFor<T>() where T : class
        {
            yield break;
        }

        public void Subscribe(Address subscriber, Type type)
        {
        }

        public void Unsubscribe(Address subscriber, Type type)
        {
        }
    }
}