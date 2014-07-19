namespace Idecom.Bus.Tests.InMemoryInfrastructure
{
    using System;
    using Addressing;
    using Implementations.UnicastBus;
    using Interfaces;
    using Transport;

    static class SortOfInMemoryQueue
    {
        public static event EventHandler<TransportMessageReceivedEventArgs> TransportMessageReceived;

        static void OnTransportMessageReceived(TransportMessageReceivedEventArgs e)
        {
            var handler = TransportMessageReceived;
            if (handler != null) handler(null, e);
        }

        public static void Enque(TransportMessage transportMessage)
        {
            OnTransportMessageReceived(new TransportMessageReceivedEventArgs(transportMessage, 1, 1));
        }
    }


    public class InMemoryTransport : ITransport
    {
        public InMemoryTransport()
        {
            SortOfInMemoryQueue.TransportMessageReceived += SortOfInMemoryQueue_TransportMessageReceived;
        }

        public int Retries { get; set; }
        public IContainer Container { get; set; }
        public Address Address { get; set; }
        public int WorkersCount { get; set; }

        public void ChangeWorkerCount(int workers)
        {
        }

        public void Send(TransportMessage transportMessage, CurrentMessageContext currentMessageContext = null)
        {
            SortOfInMemoryQueue.Enque(transportMessage);
        }

        public event EventHandler<TransportMessageReceivedEventArgs> TransportMessageReceived;
        public event EventHandler<TransportMessageFinishedEventArgs> TransportMessageFinished;

        void SortOfInMemoryQueue_TransportMessageReceived(object sender, TransportMessageReceivedEventArgs e)
        {
            if (e.TransportMessage.TargetAddress != Address)
                return;

            using (Container.BeginUnitOfWork())
            {
                TransportMessageReceived(this, new TransportMessageReceivedEventArgs(e.TransportMessage, 1, Retries));
                TransportMessageFinished(this, new TransportMessageFinishedEventArgs(e.TransportMessage));
            }
        }
    }
}