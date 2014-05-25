namespace Idecom.Bus.Interfaces.Addons.PubSub
{
    public interface ISagaStorage
    {
        void Update(string sagaId, object sagaData);
        object Get(string sagaId);
        void Close(string sagaId);
    }
}