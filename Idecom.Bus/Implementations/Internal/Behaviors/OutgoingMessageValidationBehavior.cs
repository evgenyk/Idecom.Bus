namespace Idecom.Bus.Implementations.Internal.Behaviors
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Interfaces.Behaviors;
    using UnicastBus;

    public class OutgoingMessageValidationBehavior: IBehavior
    {
        readonly OutgoingMessageContext _outgoingMessageContext;

        public OutgoingMessageValidationBehavior(OutgoingMessageContext outgoingMessageContext)
        {
            _outgoingMessageContext = outgoingMessageContext;
        }

        public void Execute(Action next, ChainExecutionContext context)
        {
            var vc = new ValidationContext(_outgoingMessageContext.Message, null, null);
            Validator.ValidateObject(_outgoingMessageContext.Message, vc);
            next();
        }
    }
}