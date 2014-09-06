namespace Idecom.Bus.Interfaces.Behaviors
{
    public interface IChainExecutor
    {
        void RunWithIt(IBehaviorChain chain, IChainExecutionContext context);
    }
}