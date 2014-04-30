namespace Idecom.Bus.Implementations.Addons.PubSub
{
    using System;
    using System.Collections.Generic;
    using Addressing;
    using Interfaces;
    using Interfaces.Addons.PubSub;
    using Transport;
    using UnicastBus;

    internal class SubscriptionDistributor : ISubscriptionDistributor
    {
        public ISubscriptionStorage Storage { get; set; }
        public Address LocalAddress { get; set; }
        public ITransport Transport { get; set; }

        public void NotifySubscribersOf<T>(object message, CurrentMessageContext currentMessageContext) where T : class
        {
            var subscribers = Storage.GetSubscribersFor<T>();
            
            foreach (var subscriber in subscribers)
                Transport.Send(message, subscriber, MessageIntent.Publish, currentMessageContext, typeof(T));
        }

        public void SubscribeTo(IEnumerable<Type> events)
        {
            foreach (var @event in events)
                Storage.Subscribe(LocalAddress, @event);
        }
    }
}