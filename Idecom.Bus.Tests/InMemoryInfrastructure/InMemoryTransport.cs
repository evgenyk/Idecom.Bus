using System;
using Idecom.Bus.Implementations.UnicastBus;
using Idecom.Bus.Interfaces;
using Idecom.Bus.Transport;

namespace Idecom.Bus.Tests.InMemoryInfrastructure
{
    public class InMemoryTransport : ITransport
    {
        public int Retries { get; set; }
        public int WorkersCount { get; set; }
        public IContainer Container { get; set; }

        public void ChangeWorkerCount(int workers)
        {
        }

        public void Send(TransportMessage transportMessage, CurrentMessageContext currentMessageContext = null)
        {
            using (Container.BeginUnitOfWork()) {
                TransportMessageReceived(this, new TransportMessageReceivedEventArgs(transportMessage, 1, Retries));
                TransportMessageFinished(this, new TransportMessageFinishedEventArgs(transportMessage));
            }
        }

        public event EventHandler<TransportMessageReceivedEventArgs> TransportMessageReceived;
        public event EventHandler<TransportMessageFinishedEventArgs> TransportMessageFinished;
    }
}