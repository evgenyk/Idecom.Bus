namespace Idecom.Bus.Tests.InMemoryInfrastructure
{
    using System;
    using Addressing;
    using Implementations.Behaviors;
    using Implementations.UnicastBus;
    using Interfaces;
    using Interfaces.Behaviors;
    using Transport;

    static class InMemoryQueue
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
            InMemoryQueue.TransportMessageReceived += SortOfInMemoryQueue_TransportMessageReceived;
        }

        public int Retries { get; set; }
        public IContainer Container { get; set; }
        public Address Address { get; set; }
        public int WorkersCount { get; set; }
        public IBehaviorChains Chains { get; set; }

        public void ChangeWorkerCount(int workers)
        {
        }

        public void Send(TransportMessage transportMessage, MessageContext messageContext = null)
        {
            InMemoryQueue.Enque(transportMessage);
        }

        public event EventHandler<TransportMessageReceivedEventArgs> TransportMessageReceived;

        void SortOfInMemoryQueue_TransportMessageReceived(object sender, TransportMessageReceivedEventArgs e)
        {
            if (e.TransportMessage.TargetAddress != Address)
                return;

            var ce = new ChainExecutor(Container);
            var chain = Chains.GetChainFor(ChainIntent.Receive);
            ce.RunWithIt(chain, new ChainExecutionContext {IncomingTransportMessage = e.TransportMessage});

            throw new NotImplementedException("Finish tyhe implementation in the behaviors");
            
//            using (Container.BeginUnitOfWork())
//            {
//                TransportMessageReceived(this, new TransportMessageReceivedEventArgs(e.TransportMessage, 1, Retries));
//            }
        }
    }
}