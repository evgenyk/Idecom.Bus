using Idecom.Bus.Interfaces;

namespace Idecom.Bus.Implementations
{
    public abstract class Saga<TState> : ISaga<TState> where TState : ISagaState
    {
        public TState SagaState { get; private set; }
    }
}