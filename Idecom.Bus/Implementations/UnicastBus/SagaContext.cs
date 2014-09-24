namespace Idecom.Bus.Implementations.UnicastBus
{
    using Interfaces.Addons.Sagas;

    public class SagaContext
    {
        public ISagaStateInstance SagaState { get; set; }
        public bool HandlerClosedSaga { get; set; }
    }
}