namespace Idecom.Bus.Interfaces.Addons.Sagas
{
    using System;
    using System.Collections.Generic;
    using Implementations.UnicastBus;
    using Transport;

    public interface ISagaManager
    {
        ISagaStateInstance Resume(Type sagaDataType, IncommingMessageContext incommingMessageContext);
        ISagaStateInstance Start(Type sagaDataType, IncommingMessageContext incommingMessageContext);
        TransportMessage PrepareSend(TransportMessage transportMessage, Dictionary<string, string> incomingHeaders, Dictionary<string, string> outgoingHeaders);
        void CloseSaga(ISagaStateInstance sagaInstance);
        void UpdateSaga(ISagaStateInstance sagaInstance);
    }
}