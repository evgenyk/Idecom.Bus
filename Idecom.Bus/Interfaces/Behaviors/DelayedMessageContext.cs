namespace Idecom.Bus.Interfaces.Behaviors
{
    using System.Collections.Generic;
    using Transport;

    public class DelayedMessageContext
    {
        readonly Queue<TransportMessage> _delayedMessages;

        public DelayedMessageContext()
        {
            _delayedMessages = new Queue<TransportMessage>();
        }

        public Queue<TransportMessage> DelayedMessages
        {
            get { return _delayedMessages; }
        }

        public void Enqueue(TransportMessage transportMessage)
        {
            DelayedMessages.Enqueue(transportMessage);
        }
    }
}