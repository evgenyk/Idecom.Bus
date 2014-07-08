using System;
using Idecom.Bus.Interfaces.Addons.PubSub;

namespace Idecom.Bus.Tests.InMemoryInfrustructure
{
    public class InMemorySagaPersister : ISagaStorage
    {
        public void Update(string sagaId, object sagaData)
        {
            throw new NotImplementedException();
        }

        public object Get(string sagaId)
        {
            throw new NotImplementedException();
        }

        public void Close(string sagaId)
        {
            throw new NotImplementedException();
        }
    }
}