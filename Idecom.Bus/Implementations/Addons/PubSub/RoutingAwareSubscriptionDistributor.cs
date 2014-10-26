namespace Idecom.Bus.Implementations.Addons.PubSub
{
    using System;
    using System.Collections.Generic;
    using Addressing;
    using Interfaces;
    using Interfaces.Addons.PubSub;
    using Transport;

    class RoutingAwareSubscriptionDistributor : ISubscriptionDistributor
    {
        public RoutingAwareSubscriptionDistributor(ISubscriptionStorage storage, Address localAddress, ITransport transport)
        {
            Storage = storage;
            LocalAddress = localAddress;
            Transport = transport;
        }

        ISubscriptionStorage Storage { get; set; }
        Address LocalAddress { get; set; }
        ITransport Transport { get; set; }

        public void NotifySubscribersOf(Type messageType, object message, bool isProcessingIncomingMessage, Action<TransportMessage> delayMessageAction)
        {
            var subscribers = Storage.GetSubscribersFor(messageType);

            foreach (var subscriber in subscribers)
            {
                var transportMessage = new TransportMessage(message, LocalAddress, subscriber, MessageIntent.Publish, messageType);
                Transport.Send(transportMessage, isProcessingIncomingMessage, delayMessageAction);
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