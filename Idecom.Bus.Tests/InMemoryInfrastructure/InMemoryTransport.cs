namespace Idecom.Bus.Tests.InMemoryInfrastructure
{
    using System.Threading.Tasks;
    using Addressing;
    using Implementations.Behaviors;
    using Implementations.UnicastBus;
    using Interfaces;
    using Interfaces.Behaviors;
    using Transport;

    public class InMemoryTransport : ITransport
    {
        public int Retries { get; set; }
        public IContainer Container { get; set; }
        public Address Address { get; set; }
        public int WorkersCount { get; set; }
        public IBehaviorChains Chains { get; set; }
        public IMessageSerializer Serializer { get; set; }
        public IChainExecutionContext GlobalExecutionContext { get; set; }

        public void Send(TransportMessage transportMessage)
        {
            Task.Factory.StartNew(() =>
                                  {
                                      var message = new TransportMessage(transportMessage.Message, transportMessage.SourceAddress, transportMessage.TargetAddress, transportMessage.Intent, transportMessage.MessageType, transportMessage.Headers); //copying the message not to have side-effects
                                      TransportMessageReceived(message);
                                  }).Wait(); //so we could test things
        }

        void TransportMessageReceived(TransportMessage transportMessage)
        {
            var ce = new ChainExecutor(Container);
            var chain = Chains.GetChainFor(ChainIntent.TransportMessageReceive);

            using (var ct = GlobalExecutionContext.Push(context =>
                                                        {
                                                            context.IncomingMessageContext = new MessageContext(transportMessage, 1, 1);
                                                        }))
            {
                ce.RunWithIt(chain, ct);
            }

        }
    }
}