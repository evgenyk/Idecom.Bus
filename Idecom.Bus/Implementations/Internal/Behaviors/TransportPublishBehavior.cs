namespace Idecom.Bus.Implementations.Internal.Behaviors
{
    using System;
    using Interfaces;
    using Interfaces.Addons.PubSub;
    using Interfaces.Behaviors;
    using UnicastBus;

    public class TransportPublishBehavior : IBehavior
    {
        readonly MessageContext _context;
        readonly OutgoingMessageContext _outgoingMessageContext;
        readonly ISubscriptionDistributor _distributor;

        public TransportPublishBehavior(MessageContext context, OutgoingMessageContext outgoingMessageContext, ISubscriptionDistributor distributor)
        {
            _context = context;
            _outgoingMessageContext = outgoingMessageContext;
            _distributor = distributor;
        }

        public void Execute(Action next)
        {
            _distributor.NotifySubscribersOf(_outgoingMessageContext.MessageType, _outgoingMessageContext.Message, _context);
            next();
        }
    }


    public class DispachMessageToHandlerBehavior: IBehavior
    {
        readonly IMessageToHandlerRoutingTable _messageToHandlerRoutingTable;
        readonly MessageContext _messageContext;

        public DispachMessageToHandlerBehavior(IMessageToHandlerRoutingTable messageToHandlerRoutingTable, MessageContext messageContext)
        {
            _messageToHandlerRoutingTable = messageToHandlerRoutingTable;
            _messageContext = messageContext;
        }

        public void Execute(Action next)
        {
            next();
        }
    }

    public class DispatcherMessageSagaBehavior: IBehavior
    {
        public void Execute(Action next)
        {
            next();
        }
    }
}