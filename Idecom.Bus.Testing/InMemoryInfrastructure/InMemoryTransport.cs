﻿namespace Idecom.Bus.Testing.InMemoryInfrastructure
{
    using System;
    using Addressing;
    using Implementations.Behaviors;
    using Implementations.UnicastBus;
    using Interfaces;
    using Interfaces.Behaviors;
    using Transport;

    public class InMemoryTransport : ITransport
    {
        public InMemoryTransport(InMemoryBroker inMemoryBroker)
        {
            InMemoryBroker = inMemoryBroker;
            InMemoryBroker.ListenToMessages(TransportMessageReceived);
        }

        public int Retries { get; set; }
        public IContainer Container { get; set; }
        public Address Address { get; set; }
        public IBehaviorChains Chains { get; set; }
        public IMessageSerializer Serializer { get; set; }
        public InMemoryBroker InMemoryBroker { get; private set; }
        public int WorkersCount { get; set; }

        public void Send(TransportMessage transportMessage, bool isProcessingIncommingMessage, Action<TransportMessage> delayMessageAction)
        {
            if (isProcessingIncommingMessage)
            {
                delayMessageAction(transportMessage);
                return;
            }
            InMemoryBroker.Enqueue(transportMessage);
        }

        void TransportMessageReceived(TransportMessage transportMessage)
        {
            var ce = new ChainExecutor(Container);
            var chain = Chains.GetChainFor(ChainIntent.TransportMessageReceive);

            using (var ct = AmbientChainContext.Current.Push(context => { context.IncomingMessageContext = new IncommingMessageContext(transportMessage, 1, 1); })) { ce.RunWithIt(chain, ct); }
        }
    }
}