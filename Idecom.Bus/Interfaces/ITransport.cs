using System;
using Idecom.Bus.Addressing;
using Idecom.Bus.Implementations.UnicastBus;
using Idecom.Bus.Transport;

namespace Idecom.Bus.Interfaces
{
    public interface ITransport
    {
        int WorkersCount { get; }
        void ChangeWorkerCount(int workers);
        void Send(object message, Address targetAddress, MessageIntent intent, CurrentMessageContext currentMessageContext);
        event EventHandler<TransportMessageReceivedEventArgs> TransportMessageReceived;
        event EventHandler<TransportMessageFinishedEventArgs> TransportMessageFinished;
    }
}