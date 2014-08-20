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
        readonly MessageContext _context;
        readonly OutgoingMessageContext _outgoingMessageContext;
        readonly ITransport _transport;
        readonly Address _localAddress;
        readonly IRoutingTable<Address> _messageRoutingTable;

        public TransportSendBehavior(MessageContext context, OutgoingMessageContext outgoingMessageContext, ITransport transport, Address localAddress, IRoutingTable<Address> messageRoutingTable)
        {
            _context = context;
            _outgoingMessageContext = outgoingMessageContext;
            _transport = transport;
            _localAddress = localAddress;
            _messageRoutingTable = messageRoutingTable;
        }

        public void Execute(Action next)
        {
            var resolveRouteFor = _messageRoutingTable.ResolveRouteFor(_outgoingMessageContext.Message.GetType());
            var transportMessage = new TransportMessage(_outgoingMessageContext.Message, _localAddress, resolveRouteFor, MessageIntent.Send);
            _transport.Send(transportMessage, _context);
            next();
        }
    }
}