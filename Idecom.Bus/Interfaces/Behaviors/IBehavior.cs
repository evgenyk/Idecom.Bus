namespace Idecom.Bus.Interfaces.Behaviors
{
    using System;

    public interface IBehavior
    {
        void Execute(Action next, IChainExecutionContext context);
    }
}