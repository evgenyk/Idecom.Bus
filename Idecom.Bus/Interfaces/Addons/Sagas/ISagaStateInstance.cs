using Idecom.Bus.Addressing;

namespace Idecom.Bus.Interfaces.Addons.Sagas
{
    public interface ISagaStateInstance
    {
        Address Endpoint { get; }
        string SagaId { get; }
        ISagaState SagaState { get; }
    }
}