namespace Idecom.Bus.Interfaces.Behaviors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Implementations.UnicastBus;
    using Transport;

    public class ChainContext
    {
        public ChainExecutionContext Current { get; set; }
    }

    public class ChainExecutionContext
    {
        readonly ChainExecutionContext _parentContext;
        object _outgoingMessage;
        Type _messageType;
        TransportMessage _incomingTransportMessage;
        readonly DelayedMessageContext _delayedMessageContext;
        SagaContext _sagaContext;
        MessageContext _incomingMessageContext;

        public ChainExecutionContext(ChainExecutionContext parentContext = null)
        {
            _parentContext = parentContext;
            _delayedMessageContext = new DelayedMessageContext();
        }

        public SagaContext SagaContext
        {
            get { return _sagaContext ?? (_parentContext == null ? null : _parentContext.SagaContext); }
            set { _sagaContext = value; }
        }

        public DelayedMessageContext DelayedMessageContext
        {
            get { return _delayedMessageContext ?? (_parentContext == null ? null : _parentContext.DelayedMessageContext); }
        }

        public object OutgoingMessage
        {
            get { return _outgoingMessage ?? (_parentContext == null ? null : _parentContext.OutgoingMessage); }
            set { _outgoingMessage = value; }
        }

        public Type OutgoingMessageType
        {
            get { return _messageType ?? (_parentContext == null ? null : _parentContext.OutgoingMessageType); }
            set { _messageType = value; }
        }

        public MethodInfo HandlerMethod { get; set; } //handler method can not be inherited as it's always local

        public void DelayMessage(TransportMessage transportMessage, ChainExecutionContext context = null)
        {
            if (context == null) { context = this; }

            if (context._parentContext != null)
                DelayMessage(transportMessage, context._parentContext);
            else context.DelayedMessageContext.Enqueue(transportMessage);
        }

        public MessageContext IncomingMessageContext
        {
            get { return _incomingMessageContext ?? (_parentContext == null ? null : _parentContext.IncomingMessageContext); ; }
            set { _incomingMessageContext = value; }
        }

        public IEnumerable<TransportMessage> GetDelayedMessages(ChainExecutionContext context = null)
        {
            if (context == null) { context = this; }

            if (context._parentContext != null)
                foreach (var transportMessage in GetDelayedMessages(context._parentContext)) yield return transportMessage;
            else
                while (context.DelayedMessageContext.DelayedMessages.Any()) yield return context.DelayedMessageContext.DelayedMessages.Dequeue();
        }
    }
}