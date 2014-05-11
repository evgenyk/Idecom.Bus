namespace Idecom.Bus.Implementations
{
    using Interfaces;
    using Interfaces.Addons.Stories;

    public abstract class Saga<TSagaState> : ISaga<TSagaState> where TSagaState : ISagaState
    {
        public IBus Bus { get; set; }
        public TSagaState Data { get; set; }

        public void CloseSaga()
        {
        }
    }
}