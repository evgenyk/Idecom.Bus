﻿namespace Idecom.Bus.Tests.InMemoryInfrastructure
{
    using System.Collections.Generic;
    using Interfaces.Addons.PubSub;

    public class InMemorySagaPersister : ISagaStorage
    {
        public Dictionary<string, object> SagaStorage;

        public InMemorySagaPersister()
        {
            SagaStorage = new Dictionary<string, object>();
        }

        public void Update(string sagaId, object sagaData)
        {
            SagaStorage[sagaId] = sagaData;
        }

        public object Get(string sagaId)
        {
            return SagaStorage.ContainsKey(sagaId) ? SagaStorage[sagaId] : null;
        }

        public void Close(string sagaId)
        {
            SagaStorage.Remove(sagaId);
        }
    }
}