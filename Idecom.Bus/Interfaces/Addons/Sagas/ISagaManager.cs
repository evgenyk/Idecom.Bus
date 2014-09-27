namespace Idecom.Bus.Interfaces.Addons.Sagas
{
    using System;
    using Implementations.UnicastBus;

    public interface ISagaManager
    {
        ISagaStateInstance Resume(Type sagaDataType, IncommingMessageContext incommingMessageContext);
        ISagaStateInstance Start(Type sagaDataType, IncommingMessageContext incommingMessageContext);
        void CloseSaga(ISagaStateInstance sagaInstance);
        void UpdateSaga(ISagaStateInstance sagaInstance);
    }
}