namespace Idecom.Bus.Implementations.Internal.Behaviors
{
    using System;
    using Addressing;
    using Interfaces;
    using Interfaces.Behaviors;
    using Transport;

    public class TransportSendLocalBehavior : IBehavior
    {
        readonly ITransport _transport;
        readonly Address _localAddress;

        public TransportSendLocalBehavior(ITransport transport, Address localAddress)
        {
            _transport = transport;
            _localAddress = localAddress;
        }

        public void Execute(Action next, ChainExecutionContext context)
        {
            var transportMessage = new TransportMessage(context.OutgoingMessage, _localAddress, _localAddress, MessageIntent.SendLocal, context.OutgoingMessage.GetType());
            _transport.Send(transportMessage);
            next();
        }
    }
}