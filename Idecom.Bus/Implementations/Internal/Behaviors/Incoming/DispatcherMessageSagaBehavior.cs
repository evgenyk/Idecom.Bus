namespace Idecom.Bus.Implementations.Internal.Behaviors.Incoming
{
    using System;
    using Interfaces;
    using Interfaces.Behaviors;
    using UnicastBus;

    public class DispatcherMessageSagaBehavior: IBehavior
    {
        readonly MessageContext _messageContext;
        readonly IContainer _container;

        public DispatcherMessageSagaBehavior(MessageContext messageContext, IContainer container)
        {
            _messageContext = messageContext;
            _container = container;
        }

        public void Execute(Action next)
        {

            next();
        }
    }
}