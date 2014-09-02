namespace Idecom.Bus.Implementations.Addons.PubSub
{
    using System;
    using System.Collections.Generic;
    using Addressing;
    using Interfaces;
    using Interfaces.Addons.PubSub;
    using Transport;
    using UnicastBus;

    class SubscriptionDistributor : ISubscriptionDistributor
    {
        public SubscriptionDistributor(ISubscriptionStorage storage, Address localAddress, ITransport transport)
        {
            Storage = storage;
            LocalAddress = localAddress;
            Transport = transport;
        }

        public ISubscriptionStorage Storage { get; private set; }
        public Address LocalAddress { get; private set; }
        public ITransport Transport { get; private set; }

        public void NotifySubscribersOf(Type messageType, object message, MessageContext messageContext, Action<TransportMessage> delayMessageAction)
        {
            var subscribers = Storage.GetSubscribersFor(messageType);

            foreach (var subscriber in subscribers)
            {
                var transportMessage = new TransportMessage(message, LocalAddress, subscriber, MessageIntent.Publish, messageType);
                if (messageContext != null)
                    delayMessageAction(transportMessage);
                else
                    Transport.Send(transportMessage);
            }
        }

        public void SubscribeTo(IEnumerable<Type> events)
        {
            foreach (var @event in events)
                Storage.Subscribe(LocalAddress, @event);
        }

        public void Unsubscribe(IEnumerable<Type> events)
        {
            foreach (var @event in events)
                Storage.Unsubscribe(LocalAddress, @event);
        }
    }
}