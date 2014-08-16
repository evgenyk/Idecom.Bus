namespace Idecom.Bus.Implementations.Internal.Behaviors
{
    using System;
    using Interfaces;
    using Interfaces.Behaviors;
    using UnicastBus;

    public class TransportSendBehavior : IBehavior
    {
        readonly CurrentMessageContext _context;
        readonly OutgoingMessageContext _outgoingMessageContext;
        readonly ITransport _transport;

        public TransportSendBehavior(CurrentMessageContext context, OutgoingMessageContext outgoingMessageContext, ITransport transport)
        {
            _context = context;
            _outgoingMessageContext = outgoingMessageContext;
            _transport = transport;
        }

        public void Execute(Action next)
        {
            _transport.Send(_outgoingMessageContext.OutgoingMessage, _context);
            next();
        }
    }
}