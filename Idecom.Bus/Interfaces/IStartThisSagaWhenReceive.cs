namespace Idecom.Bus.Interfaces
{
    public interface IStartThisSagaWhenReceive<T> : IHandle<T>
    {
    }
}