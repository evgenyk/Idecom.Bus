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
        public static void Enque(TransportMessage transportMessage)
        {

        }
    }

    public class InMemoryTransport : ITransport
    {
        public int Retries { get; set; }
        public IContainer Container { get; set; }
        public Address Address { get; set; }
        public int WorkersCount { get; set; }
        public IBehaviorChains Chains { get; set; }

        public void Send(TransportMessage transportMessage)
        {
            TransportMessageReceived(transportMessage);
        }

        void TransportMessageReceived(TransportMessage transportMessage)
        {
            var ce = new ChainExecutor(Container);
            var chain = Chains.GetChainFor(ChainIntent.TransportMessageReceive);
            ce.RunWithIt(chain, new ChainExecutionContext {IncomingTransportMessage = transportMessage});
        }
    }
}