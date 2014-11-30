namespace Idecom.Bus.Testing.InMemoryInfrastructure
{
    using System;
    using System.Collections.Generic;
    using Addressing;
    using Interfaces;
    using Interfaces.Addons.PubSub;
    using Transport;

    public class InMemorySubscriptionDescributor : ISubscriptionDistributor
    {
        public ISubscriptionStorage Storage { get; set; }
        public ITransport Transport { get; set; }
        public Address LocalAddress { get; set; }

        public void NotifySubscribersOf(Type messageType, object message, bool isProcessingIncommingMessage, Action<TransportMessage> delayMessageAction)
        {
            var subscribers = Storage.GetSubscribersFor(messageType);
            foreach (var subscriber in subscribers)
            {
                var transportMessage = new TransportMessage(message, LocalAddress, subscriber, MessageIntent.Publish, messageType);
                Transport.Send(transportMessage, isProcessingIncommingMessage, delayMessageAction);
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