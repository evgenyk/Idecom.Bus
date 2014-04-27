namespace Idecom.Bus.Interfaces
{
    public interface IHandle<in T>
    {
        void Handle(T command);
    }
}