namespace Idecom.Bus.Interfaces
{
    public interface IBus
    {
        IMessageContext CurrentMessageContext { get; }
        void Send(object message);
        void SendLocal(object message);
        void Reply(object message);
    }
}