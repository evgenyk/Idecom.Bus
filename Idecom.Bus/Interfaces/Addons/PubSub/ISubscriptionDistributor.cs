namespace Idecom.Bus.Interfaces.Addons.PubSub
{
    using System;
    using System.Collections.Generic;
    using Transport;

    public interface ISubscriptionDistributor
    {
        void NotifySubscribersOf(Type messageType, object message, bool isProcessingIncommingMessage, Action<TransportMessage> delayMessageAction);
        void SubscribeTo(IEnumerable<Type> events);
        void Unsubscribe(IEnumerable<Type> events);
    }
}