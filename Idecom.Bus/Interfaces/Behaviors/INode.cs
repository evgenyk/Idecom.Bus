namespace Idecom.Bus.Interfaces.Behavior
{
    public interface INode<T>
    {
        void AddAfter(T node);
        void AddBefore(T node);

    }
}