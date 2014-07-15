using System;
using System.Collections.Generic;
using Idecom.Bus.Addressing;
using Idecom.Bus.Interfaces.Addons.PubSub;

namespace Idecom.Bus.Tests.InMemoryInfrastructure
{
    public class InMemorySubscriptionStorage : ISubscriptionStorage
    {
        private readonly Dictionary<Type, List<Address>> _subscriptions;

        public InMemorySubscriptionStorage()
        {
            _subscriptions = new Dictionary<Type, List<Address>>();
        }

        public IEnumerable<Address> GetSubscribersFor<T>() where T : class
        {
            if (_subscriptions.ContainsKey(typeof (T)))
                return _subscriptions[typeof (T)];
            return new Address[] {};
        }

        public void Subscribe(Address subscriber, Type type)
        {
            if (_subscriptions.ContainsKey(type))
                _subscriptions[type].Add(subscriber);
            else
                _subscriptions.Add(type, new List<Address> {subscriber});
        }

        public void Unsubscribe(Address subscriber, Type type)
        {
            throw new NotImplementedException();
        }
    }
}