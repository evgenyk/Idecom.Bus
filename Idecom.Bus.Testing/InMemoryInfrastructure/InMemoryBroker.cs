namespace Idecom.Bus.Testing.InMemoryInfrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Transport;

    public class InMemoryBroker
    {
        readonly bool _startNewTasks;
        readonly List<Action<TransportMessage>> _subscriptions = new List<Action<TransportMessage>>();

        public InMemoryBroker(bool startNewTasks = true)
        {
            _startNewTasks = startNewTasks;
        }

        public void Enqueue(TransportMessage transportMessage)
        {
            var message = new TransportMessage(transportMessage.Message, transportMessage.SourceAddress, transportMessage.TargetAddress, transportMessage.Intent, transportMessage.MessageType,
                transportMessage.Headers); //copying the message not to have side-effects

            foreach (var subscription in _subscriptions)
            {
                var subscription1 = subscription;

                if (_startNewTasks)
                {
                    Task.Factory.StartNew(() => subscription1(message)).Wait();
                }
                else
                {
                    subscription1(message);
                }
            }
        }

        public void ListenToMessages(Action<TransportMessage> action)
        {
            _subscriptions.Add(action);
        }
    }
}