namespace Idecom.Bus.Interfaces
{
    using System;
    using Addressing;
    using Implementations.UnicastBus;
    using Transport;

    public interface ITransport
    {
        int WorkersCount { get; }
        void ChangeWorkerCount(int workers);
        void Send(object message, Address targetAddress, MessageIntent intent, CurrentMessageContext currentMessageContext, Type type = null);
        event EventHandler<TransportMessageReceivedEventArgs> TransportMessageReceived;
        event EventHandler<TransportMessageFinishedEventArgs> TransportMessageFinished;
    }
}