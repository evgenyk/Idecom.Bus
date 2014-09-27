namespace Idecom.Bus.Interfaces.Addons.Sagas
{
    using System;
    using System.Collections.Concurrent;
    using Implementations.UnicastBus;

    public interface ISagaManager
    {
        ISagaStateInstance Resume(Type sagaDataType, IncommingMessageContext incommingMessageContext);
        ISagaStateInstance Start(Type sagaDataType, ConcurrentDictionary<string, string> outgoingHeaders);
        void CloseSaga(ISagaStateInstance sagaInstance);
        void UpdateSaga(ISagaStateInstance sagaInstance);
    }
}