namespace Idecom.Bus.Implementations.Internal.Behaviors
{
    using System;
    using Addressing;
    using Interfaces;
    using Interfaces.Behaviors;
    using Transport;

    public class TransportSendLocalBehavior : IBehavior
    {
        readonly Address _localAddress;
        readonly ITransport _transport;

        public TransportSendLocalBehavior(ITransport transport, Address localAddress)
        {
            _transport = transport;
            _localAddress = localAddress;
        }

        public void Execute(Action next, IChainExecutionContext context)
        {
            var transportMessage = new TransportMessage(context.OutgoingMessage, _localAddress, _localAddress, MessageIntent.SendLocal, context.OutgoingMessage.GetType());
            _transport.Send(transportMessage, context.IsProcessingIncomingMessage(), message => context.DelayMessage(message));
            next();
        }
    }
}