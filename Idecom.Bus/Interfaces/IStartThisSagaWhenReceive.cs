namespace Idecom.Bus.Interfaces
{
    public interface IStartThisSagaWhenReceive<in T> : IHandle<T>
    {
    }
}