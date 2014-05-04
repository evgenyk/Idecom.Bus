using Idecom.Bus.Addressing;

namespace Idecom.Bus.Implementations.UnicastBus
{
    using System;
    using System.Collections.Generic;
    using Interfaces;
    using Transport;

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
            get
            {
                return _delayedSends;
            }
        }

        public TransportMessage TransportMessage { get; set; }

        public int Attempt { get; set; }
        public int MaxAttempts { get; set; }

        public Dictionary<string, string> Headers
        {
            get { return _headers; }
        }

        public void DelayedSend(object message, Address sourceAddress, Address targetAddress, MessageIntent intent, Type messageType)
        {
            _delayedSends.Enqueue(new DelayedSend(message, sourceAddress, targetAddress, intent, messageType));
        }

        public void StartSaga()
        {
            _headers["SAGAID"] = Guid.NewGuid().ToString();
        }
    }

    public class DelayedSend
    {
        public object Message { get; set; }
        public Address SourceAddress { get; set; }
        public Address TargetAddress { get; set; }
        public MessageIntent Intent { get; set; }
        public Type MessageType { get; set; }

        public DelayedSend(object message, Address sourceAddress, Address targetAddress, MessageIntent intent, Type messageType)
        {
            Message = message;
            SourceAddress = sourceAddress;
            TargetAddress = targetAddress;
            Intent = intent;
            MessageType = messageType;
        }
    }
}