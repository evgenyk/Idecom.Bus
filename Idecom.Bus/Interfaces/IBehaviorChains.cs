namespace Idecom.Bus.Interfaces
{
    using Behaviors;
    using Implementations.UnicastBus;

    public interface IBehaviorChains
    {
        IBehaviorChain GetChainFor(ChainIntent intent);
        void WrapWith<T>(ChainIntent intent) where T : IBehavior;
    }
}