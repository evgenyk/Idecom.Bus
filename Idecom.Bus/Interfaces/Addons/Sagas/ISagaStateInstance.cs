namespace Idecom.Bus.Interfaces.Addons.Sagas
{
    using System.Diagnostics;
    using Addressing;

    public interface ISagaStateInstance
    {
        Address Endpoint { get; }
        string SagaId { get; }
        ISagaState SagaState { get; }
    }

    [DebuggerStepThrough]
    public class SagaStateInstance : ISagaStateInstance
    {
        public SagaStateInstance(Address endpoint, string sagaId, ISagaState sagaState)
        {
            Endpoint = endpoint;
            SagaId = sagaId;
            SagaState = sagaState;
        }

        public Address Endpoint { get; private set; }
        public string SagaId { get; private set; }
        public ISagaState SagaState { get; private set; }
    }
}