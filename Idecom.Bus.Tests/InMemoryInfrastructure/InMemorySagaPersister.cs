using System.Collections.Generic;
using Idecom.Bus.Interfaces.Addons.PubSub;

namespace Idecom.Bus.Tests.InMemoryInfrastructure
{
    public class InMemorySagaPersister : ISagaStorage
    {
        public readonly Dictionary<string, object> SagaStorage;

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
            return SagaStorage[sagaId];
        }

        public void Close(string sagaId)
        {
            SagaStorage.Remove(sagaId);
        }
    }
}