namespace Idecom.Bus.Implementations.Internal.Behaviors
{
    using System;
    using Addressing;
    using Interfaces;
    using Interfaces.Behaviors;
    using Transport;

    public class ReplyBehavior : IBehavior
    {
        readonly ITransport _transport;
        readonly Address _localAddress;

        public ReplyBehavior(ITransport transport, Address localAddress)
        {
            _transport = transport;
            _localAddress = localAddress;
        }

        public void Execute(Action next, IChainExecutionContext context)
        {
            var transportMessage = new TransportMessage(context.OutgoingMessage, _localAddress, context.IncomingMessageContext.SourceAddress, MessageIntent.Reply, context.OutgoingMessage.GetType());
            _transport.Send(transportMessage, context.IsProcessingIncomingMessage(), message => context.DelayMessage(message));
            next();
        }
    }
}