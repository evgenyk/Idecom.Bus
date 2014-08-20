namespace Idecom.Bus.Implementations.UnicastBus
{
    using System.Collections.Generic;
    using System.Linq;
    using Interfaces;
    using Interfaces.Addons.Sagas;
    using Transport;

    public class MessageContext : IMessageContext
    {
        readonly Queue<TransportMessage> _delayedSends; //outgoing messages
        readonly Dictionary<string, string> _headers; //outgoing headers
        readonly ISagaManager _sagaManager;
        readonly ITransport _transport;

        public MessageContext(ITransport transport, ISagaManager sagaManager)
        {
            _transport = transport;
            _sagaManager = sagaManager;
            _delayedSends = new Queue<TransportMessage>();
            _headers = new Dictionary<string, string>();
        }

        public TransportMessage IncomingTransportMessage { get; set; } //incomming message

        public int Attempt { get; set; }
        public int MaxAttempts { get; set; }

        void SendOutgoingMessages(object sender, TransportMessageFinishedEventArgs e)
        {
            while (_delayedSends.Any())
            {
                var toSend = _delayedSends.Dequeue();
                var outgoingMessage = _sagaManager.PrepareSend(toSend, IncomingTransportMessage.Headers, _headers);
                _transport.Send(outgoingMessage);
            }
        }

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