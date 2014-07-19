namespace Idecom.Bus.Interfaces
{
    using System;
    using Implementations.UnicastBus;
    using Transport;

    public interface ITransport
    {
        int WorkersCount { get; }
        void ChangeWorkerCount(int workers);
        void Send(TransportMessage transportMessage, CurrentMessageContext currentMessageContext = null);
        event EventHandler<TransportMessageReceivedEventArgs> TransportMessageReceived;
        event EventHandler<TransportMessageFinishedEventArgs> TransportMessageFinished;
    }
}