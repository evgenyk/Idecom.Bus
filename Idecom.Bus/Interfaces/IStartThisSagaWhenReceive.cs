namespace Idecom.Bus.Interfaces
{
    public interface IStartThisSagaWhenReceive<in T> : IHandleMessage<T>
    {
    }
}