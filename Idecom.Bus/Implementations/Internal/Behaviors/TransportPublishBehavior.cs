namespace Idecom.Bus.Implementations.Internal.Behaviors
{
    using System;
    using Interfaces.Addons.PubSub;
    using Interfaces.Behaviors;
    using UnicastBus;

    public class TransportPublishBehavior : IBehavior
    {
        readonly ISubscriptionDistributor _distributor;

        public TransportPublishBehavior(ISubscriptionDistributor distributor)
        {
            _distributor = distributor;
        }

        public void Execute(Action next, ChainExecutionContext context)
        {
            _distributor.NotifySubscribersOf(context.OutgoingMessage.GetType(), context.OutgoingMessage, context.IncomingMessageContext, context);
            next();
        }
    }
}