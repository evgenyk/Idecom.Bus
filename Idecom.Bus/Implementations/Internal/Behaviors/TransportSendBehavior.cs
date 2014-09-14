namespace Idecom.Bus.Implementations.Internal.Behaviors
{
    using System;
    using Addressing;
    using Interfaces;
    using Interfaces.Behaviors;
    using Transport;

    public class TransportSendBehavior : IBehavior
    {
        readonly Address _localAddress;
        readonly IMessageToEndpointRoutingTable _messageRoutingTable;
        readonly ITransport _transport;

        public TransportSendBehavior(ITransport transport, Address localAddress, IMessageToEndpointRoutingTable messageRoutingTable)
        {
            _transport = transport;
            _localAddress = localAddress;
            _messageRoutingTable = messageRoutingTable;
        }

        public void Execute(Action next, IChainExecutionContext context)
        {
            var resolveRouteFor = _messageRoutingTable.ResolveRouteFor(context.OutgoingMessageType);

            if (resolveRouteFor == null) {
                throw new Exception(string.Format("Could not resolve a route to {0}", context.OutgoingMessageType));
            }

            var transportMessage = new TransportMessage(context.OutgoingMessage, _localAddress, resolveRouteFor, MessageIntent.Send, context.OutgoingMessageType);
            _transport.Send(transportMessage);
            next();
        }
    }
}