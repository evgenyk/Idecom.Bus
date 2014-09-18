namespace Idecom.Bus.Implementations.UnicastBus
{
    using System;
    using System.Collections.Generic;
    using Interfaces;
    using Transport;
    using Utility;

    public class MessageContext : IMessageContext
    {
        readonly Dictionary<string, string> _headers;
        TransportMessage _incomingTransportMessage;

        protected MessageContext()
        {
            _headers = new Dictionary<string, string>();
        }

        public MessageContext(TransportMessage incomingTransportMessage, int attempt, int maxAttempts) : this()
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

        public bool ContainsSagaIdForType(Type sagaDataType)
        {
            return _incomingTransportMessage.Headers.ContainsKey(SystemHeaders.SagaIdHeaderKey(sagaDataType));
        }

        public string GetSagaIdForType(Type sagaDataType)
        {
            return _incomingTransportMessage.Headers[SystemHeaders.SagaIdHeaderKey(sagaDataType)];
        }
    }
}