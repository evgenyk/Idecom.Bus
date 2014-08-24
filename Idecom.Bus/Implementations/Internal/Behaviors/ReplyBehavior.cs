namespace Idecom.Bus.Implementations.Internal.Behaviors
{
    using System;
    using Interfaces.Behaviors;

    public class ReplyBehavior : IBehavior
    {
        public void Execute(Action next)
        {
            throw new NotImplementedException();
            next();
        }
    }
}