namespace Idecom.Bus.Implementations.Internal.Behaviors
{
    using System;
    using Interfaces.Addons.PubSub;
    using Interfaces.Behaviors;
    using UnicastBus;

    public class TransportPublishBehavior : IBehavior
    {
        readonly CurrentMessageContext _context;
        readonly OutgoingMessageContext _outgoingMessageContext;
        readonly ISubscriptionDistributor _distributor;

        public TransportPublishBehavior(CurrentMessageContext context, OutgoingMessageContext outgoingMessageContext, ISubscriptionDistributor distributor)
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
}