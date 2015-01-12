namespace Idecom.Bus.Implementations.Internal.Behaviors
{
    using System;
    using Interfaces;
    using Interfaces.Behaviors;

    public class SendPendingTransportMessagesBehavior : IBehavior
    {
        readonly ITransport _transport;

        public SendPendingTransportMessagesBehavior(ITransport transport)
        {
            _transport = transport;
        }

        public void Execute(Action next, IChainExecutionContext context)
        {
            _transport.Send(context.OutgoingTransportMessage, false, null);
            next();
        }
    }
}