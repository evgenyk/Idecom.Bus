using System;

namespace Idecom.Bus.Interfaces.Addons.PubSub
{
    public interface ISagaStorage
    {
        void Update(string sagaId, object sagaData);
        object Get(Type sagaDataType, string sagaId);
    }
}