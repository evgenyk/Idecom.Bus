namespace Idecom.Bus.Interfaces
{
    public interface IHandle<T>
    {
        void Handle(T command);
    }
}