using System;
using Idecom.Bus.Implementations.UnicastBus;
using Idecom.Bus.Interfaces;
using Idecom.Bus.Transport;

namespace Idecom.Bus.Tests.InMemoryInfrustructure
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
            using (Container.BeginUnitOfWork())
            {
                TransportMessageReceived(this, new TransportMessageReceivedEventArgs(new TransportMessage(transportMessage.Message, transportMessage.TargetAddress, transportMessage.TargetAddress, transportMessage.Intent, transportMessage.Message.GetType()), 1, 1));
            }
        }

        public event EventHandler<TransportMessageReceivedEventArgs> TransportMessageReceived;
        public event EventHandler<TransportMessageFinishedEventArgs> TransportMessageFinished;
    }
}