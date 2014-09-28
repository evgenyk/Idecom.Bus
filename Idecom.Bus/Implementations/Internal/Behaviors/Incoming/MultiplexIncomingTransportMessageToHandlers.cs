namespace Idecom.Bus.Implementations.Internal.Behaviors.Incoming
{
    using System;
    using System.Linq;
    using Implementations.Behaviors;
    using Interfaces;
    using Interfaces.Behaviors;
    using UnicastBus;

    public class MultiplexIncomingTransportMessageToHandlers : IBehavior
    {
        readonly IBehaviorChains _chains;
        readonly IContainer _container;
        readonly IMessageToHandlerRoutingTable _messageToHandlerRoutingTable;

        public MultiplexIncomingTransportMessageToHandlers(IMessageToHandlerRoutingTable messageToHandlerRoutingTable, IContainer container, IBehaviorChains chains)
        {
            _messageToHandlerRoutingTable = messageToHandlerRoutingTable;
            _container = container;
            _chains = chains;
        }

        public void Execute(Action next, IChainExecutionContext context)
        {
            var handlers = _messageToHandlerRoutingTable.ResolveRouteFor(context.IncomingMessageContext.IncommingMessageType).ToList();

            if (!handlers.Any())
            {
                //TODO: throw new Exception(string.Format("Couldn't find handlers for an incoming message of type {0}", context.IncomingMessageContext.IncomingTransportMessage.MessageType));
            }

            foreach (var method in handlers)
            {
                var chain = _chains.GetChainFor(ChainIntent.IncomingMessageHandling);

                var handlerMethod = method;
                using (context = context.Push(childContext => { childContext.HandlerMethod = handlerMethod; })) { new ChainExecutor(_container).RunWithIt(chain, context); }
            }

            next();
        }
    }
}