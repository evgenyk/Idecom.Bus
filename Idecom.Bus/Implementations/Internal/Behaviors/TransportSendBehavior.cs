namespace Idecom.Bus.Implementations.Internal.Behaviors
{
    using System;
    using Addressing;
    using Interfaces;
    using Interfaces.Behaviors;
    using Transport;
    using UnicastBus;

    public class TransportSendBehavior : IBehavior
    {
        readonly OutgoingMessageContext _outgoingMessageContext;
        readonly ITransport _transport;
        readonly Address _localAddress;
        readonly IRoutingTable<Address> _messageRoutingTable;

        public TransportSendBehavior(OutgoingMessageContext outgoingMessageContext, ITransport transport, Address localAddress, IRoutingTable<Address> messageRoutingTable)
        {
            _outgoingMessageContext = outgoingMessageContext;
            _transport = transport;
            _localAddress = localAddress;
            _messageRoutingTable = messageRoutingTable;
        }

        public void Execute(Action next, ChainExecutionContext context)
        {
            var resolveRouteFor = _messageRoutingTable.ResolveRouteFor(_outgoingMessageContext.Message.GetType());
            var transportMessage = new TransportMessage(_outgoingMessageContext.Message, _localAddress, resolveRouteFor, MessageIntent.Send, _outgoingMessageContext.MessageType);
            _transport.Send(transportMessage);
            next();
        }
    }
}