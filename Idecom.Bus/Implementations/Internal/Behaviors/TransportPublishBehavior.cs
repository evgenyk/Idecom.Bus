namespace Idecom.Bus.Implementations.Internal.Behaviors
{
    using System;
    using Interfaces.Addons.PubSub;
    using Interfaces.Behaviors;
    using UnicastBus;

    public class TransportPublishBehavior : IBehavior
    {
        readonly MessageContext _context;
        readonly OutgoingMessageContext _outgoingMessageContext;
        readonly ISubscriptionDistributor _distributor;
        readonly ChainContext _chainContext;

        public TransportPublishBehavior(MessageContext context, OutgoingMessageContext outgoingMessageContext, ISubscriptionDistributor distributor, ChainContext chainContext)
        {
            _context = context;
            _outgoingMessageContext = outgoingMessageContext;
            _distributor = distributor;
            _chainContext = chainContext;
        }

        public void Execute(Action next, ChainExecutionContext context)
        {
            _distributor.NotifySubscribersOf(_outgoingMessageContext.MessageType, _outgoingMessageContext.Message, _context, _chainContext);
            next();
        }
    }
}