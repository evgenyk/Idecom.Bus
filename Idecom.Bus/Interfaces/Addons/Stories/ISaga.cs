namespace Idecom.Bus.Interfaces.Addons.Stories
{
    using System;

    public interface ISaga<out TState> where TState : ISagaState
    {
        TState SagaState { get; }
    }

    public abstract class SagaState : ISagaState
    {
        private Guid _id;

        protected SagaState()
        {
            _id = Guid.NewGuid();
        }
    }
}