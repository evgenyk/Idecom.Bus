using System;
using System.Collections.Generic;
using Idecom.Bus.Implementations.UnicastBus;
using Idecom.Bus.Transport;

namespace Idecom.Bus.Interfaces.Addons.Sagas
{
    public interface ISagaManager
    {
        ISagaStateInstance Resume(Type sagaDataType, CurrentMessageContext currentMessageContext);
        ISagaStateInstance Start(Type sagaDataType, CurrentMessageContext currentMessageContext);
        TransportMessage PrepareSend(TransportMessage transportMessage, Dictionary<string, string> incomingHeaders, Dictionary<string, string> outgoingHeaders);
    }
}