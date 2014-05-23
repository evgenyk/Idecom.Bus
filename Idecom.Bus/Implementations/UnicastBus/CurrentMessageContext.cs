using System;
using System.Collections.Generic;
using Idecom.Bus.Interfaces;
using Idecom.Bus.Transport;
using Idecom.Bus.Utility;

namespace Idecom.Bus.Implementations.UnicastBus
{
    public class CurrentMessageContext : IMessageContext
    {
        private readonly Queue<DelayedSend> _delayedSends;
        private readonly Dictionary<string, string> _headers;

        public CurrentMessageContext()
        {
            _delayedSends = new Queue<DelayedSend>();
            _headers = new Dictionary<string, string>();
        }

        public Queue<DelayedSend> DelayedSends
        {
            get { return _delayedSends; }
        }

        public Dictionary<string, string> Headers
        {
            get { return _headers; }
        }

        public TransportMessage TransportMessage { get; set; }

        public int Attempt { get; set; }
        public int MaxAttempts { get; set; }

        public void DelayedSend(TransportMessage transportMessage)
        {
            _delayedSends.Enqueue(new DelayedSend(transportMessage));
        }

        public string StartSaga()
        {
            var sagaId = ShortGuid.NewGuid();
            _headers[SystemHeaders.SAGA_ID] = sagaId;
            return sagaId;
        }

        public void ResumeSaga(string sagaId)
        {
            if (_headers.ContainsKey(SystemHeaders.SAGA_ID) && !_headers[SystemHeaders.SAGA_ID].Equals(sagaId, StringComparison.CurrentCultureIgnoreCase))
                throw new Exception("Seem like we tried to resume saga when one was already running. This might indicate a bug in Idecom.Bus.");
            _headers[SystemHeaders.SAGA_ID] = sagaId;
        }
    }

    public class DelayedSend
    {
        public DelayedSend(TransportMessage transportMessage)
        {
            TransportMessage = transportMessage;
        }

        public TransportMessage TransportMessage { get; set; }
    }
}