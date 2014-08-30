namespace Idecom.Bus.Implementations.Internal.Behaviors.Incoming
{
    using System;
    using Interfaces;
    using Interfaces.Behaviors;
    using UnicastBus;

    public class SendDelayedMessagesBehavior : IBehavior
    {
        readonly ITransport _transport;

        public SendDelayedMessagesBehavior(ITransport transport)
        {
            _transport = transport;
        }

        public void Execute(Action next, ChainExecutionContext context)
        {
            foreach (var transportMessage in context.GetDelayedMessages())
            {
                _transport.Send(transportMessage);
            }
        }
    }
}