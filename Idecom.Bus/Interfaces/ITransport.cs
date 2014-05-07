using System;
using Idecom.Bus.Implementations.UnicastBus;
using Idecom.Bus.Transport;

namespace Idecom.Bus.Interfaces
{
    public interface ITransport
    {
        int WorkersCount { get; }
        void ChangeWorkerCount(int workers);
        void Send(TransportMessage transportMessage, CurrentMessageContext currentMessageContext = null);
        event EventHandler<TransportMessageReceivedEventArgs> TransportMessageReceived;
        event EventHandler<TransportMessageFinishedEventArgs> TransportMessageFinished;
    }
}