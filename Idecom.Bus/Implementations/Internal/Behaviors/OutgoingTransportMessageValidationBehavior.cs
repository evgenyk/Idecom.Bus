namespace Idecom.Bus.Implementations.Internal.Behaviors
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Interfaces.Behaviors;

    public class OutgoingTransportMessageValidationBehavior : IBehavior
    {
        public void Execute(Action next, IChainExecutionContext context)
        {
            var vc = new ValidationContext(context.OutgoingTransportMessage, null, null);
            Validator.ValidateObject(context.OutgoingTransportMessage, vc);
            next();
        }
    }
}