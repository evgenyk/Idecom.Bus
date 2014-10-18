namespace Idecom.Bus.Interfaces
{
    using System;

    public interface IBus
    {
        IMessageContext IncomingMessageContext { get; }
        void Send(object message);
        void SendLocal(object message);
        void Reply(object message);
        void Publish<T>(Action<T> action = null) where T : class;
    }
}