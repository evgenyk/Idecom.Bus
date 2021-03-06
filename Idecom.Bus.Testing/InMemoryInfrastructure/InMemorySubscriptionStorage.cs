namespace Idecom.Bus.Testing.InMemoryInfrastructure
{
    using System;
    using System.Collections.Generic;
    using Addressing;
    using Interfaces.Addons.PubSub;

    public class InMemorySubscriptionStorage : ISubscriptionStorage
    {
        readonly Dictionary<Type, List<Address>> _subscriptions;

        public InMemorySubscriptionStorage()
        {
            _subscriptions = new Dictionary<Type, List<Address>>();
        }

        public IEnumerable<Address> GetSubscribersFor(Type type)
        {
            if (_subscriptions.ContainsKey(type))
                return _subscriptions[type];
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
            if (_subscriptions.ContainsKey(type))
                _subscriptions[type].Remove(subscriber);
        }
    }
}