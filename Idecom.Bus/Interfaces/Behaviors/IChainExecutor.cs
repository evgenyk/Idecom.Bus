namespace Idecom.Bus.Interfaces.Behaviors
{
    using Implementations.Behaviors;

    public interface IChainExecutor
    {
        void RunWithIt(BehaviorChain chain);
    }
}