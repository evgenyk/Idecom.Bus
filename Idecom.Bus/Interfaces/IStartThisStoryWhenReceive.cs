namespace Idecom.Bus.Interfaces
{
    public interface IStartThisStoryWhenReceive<in T> : IHandle<T>
    {
    }
}