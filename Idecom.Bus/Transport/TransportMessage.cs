namespace Idecom.Bus.Transport
{
    using System;
    using Addressing;

    public class TransportMessage
    {
        public TransportMessage(object message, Address sourceAddress, Address targetAddress, MessageIntent intent, Type messageType) : this(message)
        {
            SourceAddress = sourceAddress;
            TargetAddress = targetAddress;
            Intent = intent;
            MessageType = messageType;
        }

        private TransportMessage(object message)
        {
            Message = message;
        }

        public Address SourceAddress { get; private set; }
        public Address TargetAddress { get; private set; }
        public MessageIntent Intent { get; private set; }
        public Type MessageType { get; private set; }
        public object Message { get; private set; }
    }
}