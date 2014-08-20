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
            if (!messageContext.IncomingTransportMessage.Headers.ContainsKey(SystemHeaders.SagaIdHeaderKey(sagaDataType)))
                return null;

            var runningSagaId = messageContext.IncomingTransportMessage.Headers[SystemHeaders.SagaIdHeaderKey(sagaDataType)];
            var sagaState = SagaStorage.Get(runningSagaId) as ISagaState;
            return new SagaStateInstance(Address, runningSagaId, sagaState);
        }

        public ISagaStateInstance Start(Type sagaDataType, MessageContext messageContext)
        {
            var sagaId = ShortGuid.NewGuid().ToString();
            var instance = InstanceCreator.CreateInstanceOf(sagaDataType) as ISagaState;
            if (instance == null)
                throw new Exception("SagaState has to be inherited from ISagaState");

            AddSagaIdToHeaders(sagaId, sagaDataType, messageContext);

            return new SagaStateInstance(Address, sagaId, instance);
        }

        public TransportMessage PrepareSend(TransportMessage transportMessage, Dictionary<string, string> incomingHeaders, Dictionary<string, string> outgoingHeaders)
        {
            foreach (var header in incomingHeaders.Concat(outgoingHeaders).Where(keyValuePair => keyValuePair.Key.StartsWith(SystemHeaders.SagaIdPrefix)))
                transportMessage.Headers[header.Key] = header.Value;
            return transportMessage;
        }

        void AddSagaIdToHeaders(string sagaId, Type sagaDataType, MessageContext messageContext)
        {
            var headerKey = SystemHeaders.SagaIdHeaderKey(sagaDataType);
            messageContext.SetHeader(headerKey, sagaId);
        }
    }
}