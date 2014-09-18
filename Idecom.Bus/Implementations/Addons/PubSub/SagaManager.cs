namespace Idecom.Bus.Implementations.Addons.PubSub
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Addressing;
    using Interfaces;
    using Interfaces.Addons.PubSub;
    using Interfaces.Addons.Sagas;
    using Transport;
    using UnicastBus;
    using Utility;

    public class SagaManager : ISagaManager
    {
        public ISagaStorage SagaStorage { get; set; }
        public IInstanceCreator InstanceCreator { get; set; }
        public Address Address { get; set; }

        public ISagaStateInstance Resume(Type sagaDataType, MessageContext messageContext)
        {
            if (!messageContext.ContainsSagaIdForType(sagaDataType))
                return null;

            var runningSagaId = messageContext.GetSagaIdForType(sagaDataType);
            var sagaState = SagaStorage.Get(runningSagaId) as ISagaState;
            return new SagaStateInstance(Address, runningSagaId, sagaState);
        }

        public ISagaStateInstance Start(Type sagaDataType, MessageContext messageContext)
        {
            var sagaId = ShortGuid.NewGuid().ToString();
            var instance = InstanceCreator.CreateInstanceOf(sagaDataType) as ISagaState;
            if (instance == null)
                throw new Exception("SagaState has to be inherited from ISagaState");

            messageContext.SetHeader(SystemHeaders.SagaIdHeaderKey(sagaDataType), sagaId);

            return new SagaStateInstance(Address, sagaId, instance);
        }

        public TransportMessage PrepareSend(TransportMessage transportMessage, Dictionary<string, string> incomingHeaders, Dictionary<string, string> outgoingHeaders)
        {
            foreach (var header in incomingHeaders.Concat(outgoingHeaders).Where(keyValuePair => keyValuePair.Key.StartsWith(SystemHeaders.SagaIdPrefix)))
                transportMessage.Headers[header.Key] = header.Value;
            return transportMessage;
        }

        public void CloseSaga(ISagaStateInstance sagaInstance)
        {
            SagaStorage.Close(sagaInstance.SagaId);
        }

        public void UpdateSaga(ISagaStateInstance sagaInstance)
        {
            SagaStorage.Update(sagaInstance.SagaId, sagaInstance.SagaData);
        }
    }
}