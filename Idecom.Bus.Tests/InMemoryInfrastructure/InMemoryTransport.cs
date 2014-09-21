namespace Idecom.Bus.Tests.InMemoryInfrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Addressing;
    using Implementations.Behaviors;
    using Implementations.UnicastBus;
    using Interfaces;
    using Interfaces.Behaviors;
    using Transport;

    public class InMemoryBroker
    {
        readonly List<Action<TransportMessage>> _subscriptions = new List<Action<TransportMessage>>();

        public void Enqueue(TransportMessage transportMessage)
        {
            var message = new TransportMessage(transportMessage.Message, transportMessage.SourceAddress, transportMessage.TargetAddress, transportMessage.Intent, transportMessage.MessageType,
                transportMessage.Headers); //copying the message not to have side-effects

            foreach (var subscription in _subscriptions)
            {
                Action<TransportMessage> subscription1 = subscription;
                Task.Factory.StartNew(() => subscription1(message)).Wait();
            }
        }

        public void ListenToMessages(Action<TransportMessage> action)
        {
            _subscriptions.Add(action);
        }
    }

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