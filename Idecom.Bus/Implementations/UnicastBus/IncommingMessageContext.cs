namespace Idecom.Bus.Implementations.UnicastBus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Addressing;
    using Interfaces;
    using Transport;
    using Utility;

    public class IncommingMessageContext : IMessageContext
    {
        readonly TransportMessage _incomingTransportMessage;

        protected IncommingMessageContext()
        {
        }

        public IncommingMessageContext(TransportMessage incomingTransportMessage, int attempt, int maxAttempts) : this()
        {
            _incomingTransportMessage = incomingTransportMessage;
            Attempt = attempt;
            MaxAttempts = maxAttempts;
        }

        public Type IncommingMessageType => _incomingTransportMessage.MessageType ?? _incomingTransportMessage.Message.GetType();
        public Address SourceAddress => _incomingTransportMessage.SourceAddress;
        public object IncommingMessage => _incomingTransportMessage.Message;
        public IEnumerable<KeyValuePair<string, string>> IncomingHeaders => _incomingTransportMessage.Headers;

        public int Attempt { get; set; }
        public int MaxAttempts { get; set; }


        public bool ContainsSagaIdForType(Type sagaDataType)
        {
            return _incomingTransportMessage.Headers.ContainsKey(SystemHeaders.SagaIdHeaderKey(sagaDataType));
        }

        public string GetSagaIdForType(Type sagaDataType)
        {
            return _incomingTransportMessage.Headers[SystemHeaders.SagaIdHeaderKey(sagaDataType)];
        }

        public IEnumerable<KeyValuePair<string, string>> GetSagaHeaders()
        {
            return _incomingTransportMessage.Headers.Where(header => header.Key.StartsWith(SystemHeaders.SagaIdPrefix, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}