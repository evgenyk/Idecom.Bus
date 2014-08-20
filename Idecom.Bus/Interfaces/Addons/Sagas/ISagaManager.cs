namespace Idecom.Bus.Interfaces.Addons.Sagas
{
    using System;
    using System.Collections.Generic;
    using Implementations.UnicastBus;
    using Transport;

    public interface ISagaManager
    {
        ISagaStateInstance Resume(Type sagaDataType, MessageContext messageContext);
        ISagaStateInstance Start(Type sagaDataType, MessageContext messageContext);
        TransportMessage PrepareSend(TransportMessage transportMessage, Dictionary<string, string> incomingHeaders, Dictionary<string, string> outgoingHeaders);
    }
}