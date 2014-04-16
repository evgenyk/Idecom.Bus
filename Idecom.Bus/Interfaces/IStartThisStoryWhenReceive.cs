namespace Idecom.Bus.Interfaces
{
    public interface IStartThisStoryWhenReceive<in T> : IHandleMessage<T>
    {
    }
}