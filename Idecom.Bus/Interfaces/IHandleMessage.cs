namespace Idecom.Bus.Interfaces
{
    public interface IHandleMessage<in T>
    {
        void Handle(T message);
    }
}