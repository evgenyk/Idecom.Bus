namespace Idecom.Bus.Interfaces.Addons.Sagas
{
    using System.Diagnostics;
    using Addressing;

    public interface ISagaStateInstance
    {
        Address Endpoint { get; }
        string SagaId { get; }
        ISagaState SagaData { get; }
    }

    [DebuggerStepThrough]
    public class SagaStateInstance : ISagaStateInstance
    {
        public SagaStateInstance(Address endpoint, string sagaId, ISagaState sagaData)
        {
            Endpoint = endpoint;
            SagaId = sagaId;
            SagaData = sagaData;
        }

        public Address Endpoint { get; private set; }
        public string SagaId { get; private set; }
        public ISagaState SagaData { get; private set; }
    }
}