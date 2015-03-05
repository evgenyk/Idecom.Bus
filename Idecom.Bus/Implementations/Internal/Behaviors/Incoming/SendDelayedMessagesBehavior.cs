namespace Idecom.Bus.Implementations.Internal.Behaviors.Incoming
{
    using System;
    using Interfaces;
    using Interfaces.Behaviors;
    using Transport;
    using UnicastBus;

    public class SendDelayedMessagesBehavior : IBehavior
    {
        readonly IBehaviorChains _chains;
        readonly IChainExecutor _executor;

        public SendDelayedMessagesBehavior(IBehaviorChains chains, IChainExecutor executor)
        {
            _chains = chains;
            _executor = executor;
        }

        public void Execute(Action next, IChainExecutionContext context)
        {
            next();
            foreach (var transportMessage in context.GetDelayedMessages())
            {
                //we don't care if there's an incoming message context here, as that's the point we're sending pending messages
                if (context.IncomingMessageContext != null)
                {
                    foreach (var sagaHeader in context.IncomingMessageContext.GetSagaHeaders())
                    {
                        transportMessage.Headers[sagaHeader.Key] = sagaHeader.Value;
                    }
                }
                foreach (var outgoingHeader in context.OutgoingHeaders)
                {
                    transportMessage.Headers[outgoingHeader.Key] = outgoingHeader.Value;
                }


                TransportMessage message = transportMessage;
                using (var executionContext = context.Push(innerContext =>
                {
                    innerContext.OutgoingTransportMessage = message;
                }))
                {
                    _executor.RunWithIt(_chains.GetChainFor(ChainIntent.SendDelayed), executionContext);
                }
            }
        }
    }
}