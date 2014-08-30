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
        readonly IMessageToHandlerRoutingTable _messageToHandlerRoutingTable;

        public MultiplexIncomingTransportMessageToHandlers(IMessageToHandlerRoutingTable messageToHandlerRoutingTable, IContainer container, IBehaviorChains chains)
        {
            _messageToHandlerRoutingTable = messageToHandlerRoutingTable;
            _container = container;
            _chains = chains;
        }
        public void Execute(Action next, ChainExecutionContext context)
        {
            var handlers = _messageToHandlerRoutingTable.ResolveRouteFor(context.IncomingTransportMessage.MessageType);
            foreach (var method in handlers)
            {
                var chain = _chains.GetChainFor(ChainIntent.IncomingMessageHandling);
                new ChainExecutor(_container).RunWithIt(chain, new ChainExecutionContext(context) { HandlerMethod = method });
            }

            next();
        }
    }
}