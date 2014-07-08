using Idecom.Bus.Interfaces.Addons.PubSub;

namespace Idecom.Bus.Tests.InMemoryInfrustructure
{
    public class InMemorySagaPersister : ISagaStorage
    {
        public void Update(string sagaId, object sagaData)
        {
        }

        public object Get(string sagaId)
        {
            return null;
        }

        public void Close(string sagaId)
        {
        }
    }
}