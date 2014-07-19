namespace Idecom.Bus.Interfaces.Addons.Sagas
{
    using System;
    using System.Collections.Generic;
    using Implementations.UnicastBus;
    using Transport;

    public interface ISagaManager
    {
        ISagaStateInstance Resume(Type sagaDataType, CurrentMessageContext currentMessageContext);
        ISagaStateInstance Start(Type sagaDataType, CurrentMessageContext currentMessageContext);
        TransportMessage PrepareSend(TransportMessage transportMessage, Dictionary<string, string> incomingHeaders, Dictionary<string, string> outgoingHeaders);
    }
}