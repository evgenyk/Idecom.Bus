using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Idecom.Bus.Addressing;
using Idecom.Bus.Implementations.UnicastBus;
using Idecom.Bus.Interfaces;
using Idecom.Bus.Interfaces.Addons.PubSub;
using Idecom.Bus.Interfaces.Addons.Sagas;
using Idecom.Bus.Transport;
using Idecom.Bus.Utility;

namespace Idecom.Bus.Tests.InMemoryInfrastructure
{
    public class InMemorySagaManager: ISagaManager
    {
        public ISagaStorage SagaStorage { get; set; }
        public IInstanceCreator InstanceCreator { get; set; }
        public Address Address { get; set; }

        public ISagaStateInstance Resume(Type sagaDataType, CurrentMessageContext currentMessageContext)
        {
            var runningSagaId = currentMessageContext.TransportMessage.Headers[SystemHeaders.SagaIdHeaderKey(sagaDataType)];
            var sagaState = SagaStorage.Get(runningSagaId) as ISagaState;
            return new SagaStateInstance(Address, runningSagaId, sagaState);
        }

        public ISagaStateInstance Start(Type sagaDataType, CurrentMessageContext currentMessageContext)
        {
            var sagaId = ShortGuid.NewGuid().ToString();
            var instance = InstanceCreator.CreateInstanceOf(sagaDataType) as ISagaState;
            if (instance == null)
                throw new Exception("SagaState has to be inherited from ISagaState");

            AddSagaIdToHeaders(sagaId, sagaDataType, currentMessageContext);

            return new SagaStateInstance(Address, sagaId, instance);
        }

        public TransportMessage PrepareSend(TransportMessage transportMessage, Dictionary<string, string> incomingHeaders, Dictionary<string, string> outgoingHeaders)
        {
            incomingHeaders.Concat(outgoingHeaders).Where(keyValuePair => keyValuePair.Key.StartsWith(SystemHeaders.SagaIdPrefix)).ForEach(x =>
            {
                transportMessage.Headers[x.Key] = x.Value;
            });
            return transportMessage;
        }

        private void AddSagaIdToHeaders(string sagaId, Type sagaDataType, CurrentMessageContext currentMessageContext)
        {
            var headerKey = SystemHeaders.SagaIdHeaderKey(sagaDataType);
            currentMessageContext.SetHeader(headerKey, sagaId);
        }
    }
}