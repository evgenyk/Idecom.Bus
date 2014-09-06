namespace Idecom.Bus.Implementations.Internal.Behaviors
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Interfaces.Behaviors;

    public class OutgoingMessageValidationBehavior: IBehavior
    {
        public void Execute(Action next, IChainExecutionContext context)
        {
            var vc = new ValidationContext(context.OutgoingMessage, null, null);
            Validator.ValidateObject(context.OutgoingMessage, vc);
            next();
        }
    }
}