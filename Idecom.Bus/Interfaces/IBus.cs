namespace Idecom.Bus.Interfaces
{
    using System;

    public interface IBus
    {
        IMessageContext CurrentMessageContext { get; }
        void Send(object message);
        void SendLocal(object message);
        void Reply(object message);
        void Raise<T>(Action<T> action) where T : class;
    }
}