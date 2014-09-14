namespace Idecom.Bus.Implementations.Internal.Behaviors
{
    using System;
    using Interfaces.Addons.PubSub;
    using Interfaces.Behaviors;

    public class TransportPublishBehavior : IBehavior
    {
        readonly ISubscriptionDistributor _distributor;

        public TransportPublishBehavior(ISubscriptionDistributor distributor)
        {
            _distributor = distributor;
        }

        public void Execute(Action next, IChainExecutionContext context)
        {
            var incomingMessageContext = context.IncomingMessageContext;
            _distributor.NotifySubscribersOf(context.OutgoingMessageType, context.OutgoingMessage, incomingMessageContext, transportMessage => context.DelayMessage(transportMessage));
            next();
        }
    }
}