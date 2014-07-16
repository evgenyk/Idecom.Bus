using System;
using Idecom.Bus.Implementations.UnicastBus;
using Idecom.Bus.Interfaces;
using Idecom.Bus.Transport;

namespace Idecom.Bus.Tests.InMemoryInfrastructure
{
    static class SortOfInMemoryQueue
    {
        public static event EventHandler<TransportMessageReceivedEventArgs> TransportMessageReceived;

        private static void OnTransportMessageReceived(TransportMessageReceivedEventArgs e)
        {
            EventHandler<TransportMessageReceivedEventArgs> handler = TransportMessageReceived;
            if (handler != null) handler(null, e);
        }

        public static void Enque(TransportMessage transportMessage)
        {
            OnTransportMessageReceived(new TransportMessageReceivedEventArgs(transportMessage, 1, 1));
        }
    }


    public class InMemoryTransport : ITransport
    {
        public int Retries { get; set; }
        public int WorkersCount { get; set; }
        public IContainer Container { get; set; }

        public InMemoryTransport()
        {
            SortOfInMemoryQueue.TransportMessageReceived += SortOfInMemoryQueue_TransportMessageReceived;
        }

        void SortOfInMemoryQueue_TransportMessageReceived(object sender, TransportMessageReceivedEventArgs e)
        {
            using (Container.BeginUnitOfWork())
            {
                TransportMessageReceived(this, new TransportMessageReceivedEventArgs(e.TransportMessage, 1, Retries));
                TransportMessageFinished(this, new TransportMessageFinishedEventArgs(e.TransportMessage));
            }
        }

        public void ChangeWorkerCount(int workers)
        {
        }

        public void Send(TransportMessage transportMessage, CurrentMessageContext currentMessageContext = null)
        {
            SortOfInMemoryQueue.Enque(transportMessage);
        }

        public event EventHandler<TransportMessageReceivedEventArgs> TransportMessageReceived;
        public event EventHandler<TransportMessageFinishedEventArgs> TransportMessageFinished;
    }
}