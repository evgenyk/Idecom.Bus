namespace Idecom.Bus.Implementations.Internal.Behaviors.Incoming
{
    using System;
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
        public void Execute(Action next, ChainExecutionContext context)
        {
            var handlers = _messageToHandlerRoutingTable.ResolveRouteFor(_messageContext.IncomingTransportMessage.MessageType);
            foreach (var method in handlers)
            {
                var chain = _chains.GetChainFor(ChainIntent.IncomingMessageHandling);
                new ChainExecutor(_container).RunWithIt(chain, new ChainExecutionContext(context) { HandlerMethod = method });
            }

            next();
        }
    }
}