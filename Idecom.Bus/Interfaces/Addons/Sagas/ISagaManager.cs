using System;
using Idecom.Bus.Implementations.UnicastBus;

namespace Idecom.Bus.Interfaces.Addons.Sagas
{
    public interface ISagaManager
    {
        ISagaStateInstance Resume(Type sagaDataType, CurrentMessageContext currentMessageContext);
        ISagaStateInstance Start(Type sagaDataType, CurrentMessageContext currentMessageContext);
    }
}