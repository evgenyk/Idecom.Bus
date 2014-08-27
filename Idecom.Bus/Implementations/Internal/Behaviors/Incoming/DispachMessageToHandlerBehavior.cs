namespace Idecom.Bus.Implementations.Internal.Behaviors.Incoming
{
    using System;
    using System.Reflection;
    using Interfaces;
    using Interfaces.Behaviors;
    using UnicastBus;

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

        public void Execute(Action next, ChainExecutionContext context)
        {
            var method = _context.Method;
            var handler = _container.Resolve(method.DeclaringType);
            method.Invoke(handler, new[] { _messageContext.IncomingTransportMessage.Message });
            next();
        }

    }
}