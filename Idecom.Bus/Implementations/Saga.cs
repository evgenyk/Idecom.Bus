namespace Idecom.Bus.Implementations
{
    using Interfaces;
    using Interfaces.Addons.Stories;

    public abstract class Saga<TState> : ISaga<TState> where TState : ISagaState
    {
        public IBus Bus { get; set; }
        public TState State { get; set; }

        public void CloseSaga()
        {
        }
    }
}