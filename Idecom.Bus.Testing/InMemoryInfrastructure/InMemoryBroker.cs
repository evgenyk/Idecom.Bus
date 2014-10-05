namespace Idecom.Bus.Testing.InMemoryInfrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
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
                var subscription1 = subscription;
                Task.Factory.StartNew(() => subscription1(message)).Wait();
            }
        }

        public void ListenToMessages(Action<TransportMessage> action)
        {
            _subscriptions.Add(action);
        }
    }
}