namespace Idecom.Bus.Implementations.Internal.Behaviors.Incoming
{
    using System;
    using Interfaces;
    using Interfaces.Behaviors;

    public class SendDelayedMessagesBehavior : IBehavior
    {
        readonly ITransport _transport;

        public SendDelayedMessagesBehavior(ITransport transport)
        {
            _transport = transport;
        }

        public void Execute(Action next, IChainExecutionContext context)
        {
            next();
            foreach (var transportMessage in context.GetDelayedMessages())
            {
                //we fon't cere if there's an incoming message context here, as that's the point we're sending pending messages
                if (context.IncomingMessageContext != null) {
                    foreach (var sagaHeader in context.IncomingMessageContext.GetSagaHeaders()) { transportMessage.Headers[sagaHeader.Key] = sagaHeader.Value; }
                }
                foreach (var outgoingHeader in context.OutgoingHeaders) { transportMessage.Headers[outgoingHeader.Key] = outgoingHeader.Value; }

                _transport.Send(transportMessage, false, null);
            }
        }
    }
}