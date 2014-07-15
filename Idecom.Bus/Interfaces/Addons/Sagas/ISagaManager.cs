using System;

namespace Idecom.Bus.Interfaces.Addons.Sagas
{
    public interface ISagaManager
    {
        ISagaStateInstance Resume(Type sagaDataType);
        ISagaStateInstance Start();
    }
}