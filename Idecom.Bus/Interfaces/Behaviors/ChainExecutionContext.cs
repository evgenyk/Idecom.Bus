namespace Idecom.Bus.Interfaces.Behaviors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Implementations.UnicastBus;
    using Transport;

    public class ChainContext
    {
        public ChainExecutionContext Current { get; set; }
    }

    public class ChainExecutionContext
    {
        readonly ThreadLocal<ChainExecutionContext> _parentContext;
        readonly ThreadLocal<DelayedMessageContext> _delayedMessageContext;

        object _outgoingMessage;
        Type _messageType;
        SagaContext _sagaContext;
        ThreadLocal<MessageContext> _incomingMessageContext;

        public ChainExecutionContext(ChainExecutionContext parentContext = null)
        {
            _parentContext = parentContext != null ? new ThreadLocal<ChainExecutionContext>(() => parentContext) : null;
            _delayedMessageContext = new ThreadLocal<DelayedMessageContext>(() => new DelayedMessageContext());
        }

        public SagaContext SagaContext
        {
            get { return _sagaContext ?? (_parentContext == null ? null : _parentContext.Value.SagaContext); }
            set { _sagaContext = value; }
        }

        public DelayedMessageContext DelayedMessageContext
        {

            get { return _delayedMessageContext.Value ?? (_parentContext.IsValueCreated ? null : _parentContext.Value.DelayedMessageContext); }
        }

        public object OutgoingMessage
        {
            get { return _outgoingMessage ?? (_parentContext.IsValueCreated ? null : _parentContext.Value.OutgoingMessage); }
            set { _outgoingMessage = value; }
        }

        public Type OutgoingMessageType
        {
            get { return _messageType ?? (_parentContext.IsValueCreated ? null : _parentContext.Value.OutgoingMessageType); }
            set { _messageType = value; }
        }

        public MethodInfo HandlerMethod { get; set; } //handler method can not be inherited as it's always local

        public void DelayMessage(TransportMessage transportMessage, ChainExecutionContext context = null)
        {
            if (context == null) { context = this; }

            if (context._parentContext != null)
                DelayMessage(transportMessage, context._parentContext.Value);

            else context.DelayedMessageContext.Enqueue(transportMessage);
        }

        public MessageContext IncomingMessageContext
        {
            get
            {
                if (_incomingMessageContext != null) 
                    return _incomingMessageContext.Value;
                else 
                    return _parentContext == null ? null : _parentContext.Value.IncomingMessageContext;
            }
            set { _incomingMessageContext = new ThreadLocal<MessageContext>(() => value); }
        }

        public IEnumerable<TransportMessage> GetDelayedMessages(ChainExecutionContext context = null)
        {
            if (context == null) { context = this; }

            if (context._parentContext != null)
                foreach (var transportMessage in GetDelayedMessages(context._parentContext.Value)) yield return transportMessage;
            else
                while (context.DelayedMessageContext.DelayedMessages.Any()) yield return context.DelayedMessageContext.DelayedMessages.Dequeue();
        }
    }
}