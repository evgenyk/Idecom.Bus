namespace Idecom.Bus.Transport
{
    using System;
    using System.Collections.Generic;
    using Addressing;

    public class TransportMessage
    {
        public TransportMessage(object message, Address sourceAddress, Address targetAddress, MessageIntent intent, Type messageType, Dictionary<string, string> headers)
            : this(message, headers)
        {
            SourceAddress = sourceAddress;
            TargetAddress = targetAddress;
            Intent = intent;
            MessageType = messageType;
        }

        private TransportMessage(object message, Dictionary<string, string> headers)
        {
            Message = message;
            Headers = new Dictionary<string, string>(headers, StringComparer.CurrentCultureIgnoreCase);
        }

        public Address SourceAddress { get; private set; }
        public Address TargetAddress { get; private set; }
        public MessageIntent Intent { get; private set; }
        public Type MessageType { get; private set; }
        public object Message { get; private set; }
        public Dictionary<string, string> Headers { get; private set; }
    }
}