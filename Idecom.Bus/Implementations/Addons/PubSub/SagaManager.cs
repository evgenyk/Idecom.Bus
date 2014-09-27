namespace Idecom.Bus.Implementations.Addons.PubSub
{
    using System;
    using Addressing;
    using Interfaces;
    using Interfaces.Addons.PubSub;
    using Interfaces.Addons.Sagas;
    using UnicastBus;
    using Utility;

    public class SagaManager : ISagaManager
    {
        public ISagaStorage SagaStorage { get; set; }
        public IInstanceCreator InstanceCreator { get; set; }
        public Address Address { get; set; }

        public ISagaStateInstance Resume(Type sagaDataType, IncommingMessageContext incommingMessageContext)
        {
            if (!incommingMessageContext.ContainsSagaIdForType(sagaDataType))
                return null;

            var runningSagaId = incommingMessageContext.GetSagaIdForType(sagaDataType);
            var sagaState = SagaStorage.Get(runningSagaId) as ISagaState;
            return new SagaStateInstance(Address, runningSagaId, sagaState);
        }

        public ISagaStateInstance Start(Type sagaDataType, IncommingMessageContext incommingMessageContext)
        {
            var sagaId = ShortGuid.NewGuid().ToString();
            var instance = InstanceCreator.CreateInstanceOf(sagaDataType) as ISagaState;
            if (instance == null)
                throw new Exception("SagaState has to be inherited from ISagaState");

            incommingMessageContext.SetHeader(SystemHeaders.SagaIdHeaderKey(sagaDataType), sagaId);

            return new SagaStateInstance(Address, sagaId, instance);
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