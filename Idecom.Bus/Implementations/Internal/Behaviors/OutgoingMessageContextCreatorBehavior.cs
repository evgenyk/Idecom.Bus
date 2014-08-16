namespace Idecom.Bus.Implementations.Internal.Behaviors
{
    using System;
    using Interfaces;
    using Interfaces.Behaviors;
    using UnicastBus;

    public class OutgoingMessageContextCreatorBehavior : IBehavior
    {
        readonly IContainer _container;

        public OutgoingMessageContextCreatorBehavior(IContainer container)
        {
            _container = container;
        }

        public IChainExecutionContext ChainExecutionContext { get; set; }

        public void Execute(Action next)
        {
            var outgoingMessage = _container.Resolve<OutgoingMessageContext>();
            outgoingMessage.OutgoingMessage = ChainExecutionContext.OutgoingMessage;
        }
    }
}