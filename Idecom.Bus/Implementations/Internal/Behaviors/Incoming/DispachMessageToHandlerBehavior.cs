namespace Idecom.Bus.Implementations.Internal.Behaviors.Incoming
{
    using System;
    using System.Reflection;
    using Implementations.Behaviors;
    using Interfaces;
    using Interfaces.Behaviors;
    using UnicastBus;

    public class MultiplexIncomingTransportMessageToHandlers : IBehavior
    {
        readonly IContainer _container;
        readonly IBehaviorChains _chains;
        readonly MessageContext _messageContext;
        readonly IMessageToHandlerRoutingTable _messageToHandlerRoutingTable;

        public MultiplexIncomingTransportMessageToHandlers(IMessageToHandlerRoutingTable messageToHandlerRoutingTable, MessageContext messageContext, IContainer container, IBehaviorChains chains)
        {
            _messageToHandlerRoutingTable = messageToHandlerRoutingTable;
            _messageContext = messageContext;
            _container = container;
            _chains = chains;
        }
        public void Execute(Action next)
        {
            var handlers = _messageToHandlerRoutingTable.ResolveRouteFor(_messageContext.IncomingTransportMessage.MessageType);
            foreach (var method in handlers)
            {
                var chain = _chains.GetChainFor(ChainIntent.TransportMessageReceive);
                new ChainExecutor(_container).RunWithIt(chain, new ChainExecutionContext {HandlerMethod = method});
            }

            next();
        }
    }

    public class DispachMessageToHandlerBehavior : IBehavior
    {
        readonly HandlerContext _context;
        readonly IContainer _container;
        readonly MessageContext _messageContext;

        public DispachMessageToHandlerBehavior(HandlerContext context, IContainer container, MessageContext messageContext)
        {
            _context = context;
            _container = container;
            _messageContext = messageContext;
        }

        public void Execute(Action next)
        {
            var method = _context.Method;
            var handler = _container.Resolve(method.DeclaringType);
            method.Invoke(handler, new[] { _messageContext.IncomingTransportMessage.Message });
            next();
        }
    }
}