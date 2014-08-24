namespace Idecom.Bus.Implementations.Internal.Behaviors
{
    using System;
    using Addressing;
    using Interfaces;
    using Interfaces.Behaviors;
    using Transport;
    using UnicastBus;

    public class TransportSendLocalBehavior : IBehavior
    {
        readonly OutgoingMessageContext _outgoingMessageContext;
        readonly ITransport _transport;
        readonly Address _localAddress;

        public TransportSendLocalBehavior(OutgoingMessageContext outgoingMessageContext, ITransport transport, Address localAddress)
        {
            _outgoingMessageContext = outgoingMessageContext;
            _transport = transport;
            _localAddress = localAddress;
        }

        public void Execute(Action next)
        {
            var transportMessage = new TransportMessage(_outgoingMessageContext.Message, _localAddress, _localAddress, MessageIntent.SendLocal, _outgoingMessageContext.MessageType);
            _transport.Send(transportMessage);
            next();
        }
    }
}