namespace Idecom.Bus.Implementations.UnicastBus
{
    using System.Collections.Generic;
    using Interfaces;
    using Transport;

    public class MessageContext : IMessageContext
    {
        readonly Queue<TransportMessage> _delayedSends;
        readonly Dictionary<string, string> _headers;

        public MessageContext()
        {
            _delayedSends = new Queue<TransportMessage>();
            _headers = new Dictionary<string, string>();
        }

        public TransportMessage IncomingTransportMessage { get; set; }

        public int Attempt { get; set; }
        public int MaxAttempts { get; set; }

        public void DelayedSend(TransportMessage transportMessage)
        {
            _delayedSends.Enqueue(transportMessage);
        }

        public void SetHeader(string key, string value)
        {
            _headers[key] = value;
        }

        public string GetHeader(string key)
        {
            return _headers.ContainsKey(key) ? _headers[key] : null;
        }
    }
}