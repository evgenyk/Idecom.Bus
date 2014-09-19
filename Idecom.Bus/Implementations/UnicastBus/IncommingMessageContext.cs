namespace Idecom.Bus.Implementations.UnicastBus
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Addressing;
    using Interfaces;
    using Transport;
    using Utility;

    public class IncommingMessageContext : IMessageContext
    {
        readonly Dictionary<string, string> _headers;
        TransportMessage _incomingTransportMessage;

        protected IncommingMessageContext()
        {
            _headers = new Dictionary<string, string>();
        }

        public IncommingMessageContext(TransportMessage incomingTransportMessage, int attempt, int maxAttempts) : this()
        {
            _incomingTransportMessage = incomingTransportMessage;
            Attempt = attempt;
            MaxAttempts = maxAttempts;
        }

        public int Attempt { get; set; }
        public int MaxAttempts { get; set; }

        public void SetHeader(string key, string value)
        {
            _headers[key] = value;
        }

        public string GetHeader(string key)
        {
            return _headers.ContainsKey(key) ? _headers[key] : null;
        }

        public Type IncommingMessageType
        {
            get { return _incomingTransportMessage.MessageType ?? _incomingTransportMessage.Message.GetType(); }
        }

        public Address SourceAddress
        {
            get { return _incomingTransportMessage.SourceAddress; }
        }

        public object IncommingMessage
        {
            get { return _incomingTransportMessage.Message; }
        }

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
            return _headers.Where(header => header.Key.StartsWith(SystemHeaders.SagaIdPrefix, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}