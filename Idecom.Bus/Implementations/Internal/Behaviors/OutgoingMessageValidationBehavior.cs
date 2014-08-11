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

        public void Execute(Action next)
        {
            var vc = new ValidationContext(_outgoingMessageContext.OutgoingMessage.Message, null, null);
            Validator.ValidateObject(_outgoingMessageContext.OutgoingMessage.Message, vc);
        }
    }
}