namespace Idecom.Bus.Implementations.UnicastBus
{
    using System;
    using System.Collections.Generic;
    using Interfaces;
    using Transport;

    public class MessageContext : IMessageContext
    {
        readonly Dictionary<string, string> _headers;

        protected MessageContext()
        {
            _headers = new Dictionary<string, string>();
        }

        public MessageContext(TransportMessage incomingTransportMessage, int attempt, int maxAttempts) : this()
        {
            IncomingTransportMessage = incomingTransportMessage;
            Attempt = attempt;
            MaxAttempts = maxAttempts;
        }

        public TransportMessage IncomingTransportMessage { get; set; }

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
            get { return IncomingTransportMessage.MessageType ?? IncomingTransportMessage.Message.GetType(); }
        }

    }
}