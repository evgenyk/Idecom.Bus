using System;
using System.Collections.Generic;
using Idecom.Bus.Addressing;
using Idecom.Bus.Implementations.UnicastBus;
using Idecom.Bus.Transport;

namespace Idecom.Bus.Interfaces
{
    public interface ITransport
    {
        int WorkersCount { get; }
        void Start(Address localAddress, IEnumerable<Address> targetQueues, int workersCount, int retries, IMessageSerializer serializer);
        void Stop();
        void ChangeWorkerCount(int workers);
        void Send(object message, Address sourceAddress, Address targetAddress, MessageIntent intent, CurrentMessageContext currentMessageContext);
        event EventHandler<TransportMessageReceivedEventArgs> TransportMessageReceived;
        event EventHandler<TransportMessageFinishedEventArgs> TransportMessageFinished;
    }
}