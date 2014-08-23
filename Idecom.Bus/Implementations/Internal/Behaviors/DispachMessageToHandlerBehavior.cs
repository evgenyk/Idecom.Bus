namespace Idecom.Bus.Implementations.Internal.Behaviors
{
    using System;
    using Interfaces;
    using Interfaces.Behaviors;
    using UnicastBus;

    public class DispachMessageToHandlerBehavior : IBehavior
    {
        readonly IContainer _container;
        readonly MessageContext _messageContext;
        readonly IMessageToHandlerRoutingTable _messageToHandlerRoutingTable;

        public DispachMessageToHandlerBehavior(IMessageToHandlerRoutingTable messageToHandlerRoutingTable, MessageContext messageContext, IContainer container)
        {
            _messageToHandlerRoutingTable = messageToHandlerRoutingTable;
            _messageContext = messageContext;
            _container = container;
        }

        public void Execute(Action next)
        {
            var handlers = _messageToHandlerRoutingTable.ResolveRouteFor(_messageContext.IncomingTransportMessage.MessageType);

            
            foreach (var method in handlers)
            {
                var handler = _container.Resolve(method.DeclaringType);
                method.Invoke(handler, new[] {_messageContext.IncomingTransportMessage.Message});
            }

            next();
        }
    }
}