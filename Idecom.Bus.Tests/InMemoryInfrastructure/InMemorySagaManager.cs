using System;
using Idecom.Bus.Addressing;
using Idecom.Bus.Implementations.UnicastBus;
using Idecom.Bus.Interfaces;
using Idecom.Bus.Interfaces.Addons.PubSub;
using Idecom.Bus.Interfaces.Addons.Sagas;
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

            return null;
        }

        public ISagaStateInstance Start(Type sagaDataType, CurrentMessageContext currentMessageContext)
        {
            var sagaId = ShortGuid.NewGuid().ToString();
            var instance = InstanceCreator.CreateInstanceOf(sagaDataType) as ISagaState;
            if (instance == null)
                throw new Exception("SagaState has to be inherited from ISagaState");
            return new SagaStateInstance(Address, sagaId, instance);
        }
    }
}