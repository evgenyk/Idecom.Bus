using Idecom.Bus.Addressing;

namespace Idecom.Bus.Implementations.UnicastBus
{
    using System;
    using System.Collections.Generic;
    using Interfaces;
    using Transport;

    public class CurrentMessageContext : IMessageContext
    {
        private readonly Queue<DelayedSend> _delayedActions;
        private Dictionary<string, string> _systemHeaders;

        public CurrentMessageContext()
        {
            _delayedActions = new Queue<DelayedSend>();
            _systemHeaders = new Dictionary<string, string>();
        }

        public Queue<DelayedSend> DelayedActions
        {
            get { return _delayedActions; }
        }

        public TransportMessage TransportMessage { get; set; }

        public int Attempt { get; set; }
        public int MaxAttempts { get; set; }

        public void DelayedSend(object message, Address sourceAddress, Address targetAddress, MessageIntent intent, Type messageType)
        {
            _delayedActions.Enqueue(new DelayedSend(message, sourceAddress, targetAddress, intent, messageType, _systemHeaders));
        }

        public void StartSaga()
        {
            _systemHeaders["SAGA_ID"] = Guid.NewGuid().ToString();
        }
    }

    public class DelayedSend
    {
        public object Message { get; set; }
        public Address SourceAddress { get; set; }
        public Address TargetAddress { get; set; }
        public MessageIntent Intent { get; set; }
        public Type MessageType { get; set; }
        public Guid SagaId { get; set; }

        public DelayedSend(object message, Address sourceAddress, Address targetAddress, MessageIntent intent, Type messageType, Guid sagaId)
        {
            Message = message;
            SourceAddress = sourceAddress;
            TargetAddress = targetAddress;
            Intent = intent;
            MessageType = messageType;
            SagaId = sagaId;
        }
    }
}