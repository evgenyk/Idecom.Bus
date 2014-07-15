using System;
using Idecom.Bus.Interfaces.Addons.Sagas;

namespace Idecom.Bus.Tests.InMemoryInfrastructure
{
    public class InMemorySagaManager: ISagaManager
    {
        public ISagaStateInstance Resume(Type sagaDataType)
        {
            return null;
        }

        public ISagaStateInstance Start()
        {
            return null;
        }
    }
}