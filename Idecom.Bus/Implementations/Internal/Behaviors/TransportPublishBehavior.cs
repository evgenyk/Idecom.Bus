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
            _distributor.NotifySubscribersOf(context.OutgoingMessageType, context.OutgoingMessage, context.IsProcessingIncomingMessage(), transportMessage => context.DelayMessage(transportMessage));
            next();
        }
    }
}