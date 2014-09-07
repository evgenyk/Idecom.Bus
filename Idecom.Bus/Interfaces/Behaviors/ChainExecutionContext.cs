namespace Idecom.Bus.Interfaces.Behaviors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Implementations.UnicastBus;
    using Transport;

    public interface IChainExecutionContext: IDisposable
    {
        MessageContext IncomingMessageContext { get; set; }
        SagaContext SagaContext { get; set; }
        DelayedMessageContext DelayedMessageContext { get; }
        object OutgoingMessage { get; set; }
        Type OutgoingMessageType { get; set; }
        MethodInfo HandlerMethod { get; set; }
        IChainExecutionContext Push(Action<IChainExecutionContext> populator);
        void DelayMessage(TransportMessage transportMessage, ChainExecutionContext context = null);
        IEnumerable<TransportMessage> GetDelayedMessages(ChainExecutionContext context = null);
    }

    public class ChainExecutionContext : IChainExecutionContext
    {
        readonly ThreadLocal<ChainExecutionContext> _parentContext;
        readonly ThreadLocal<DelayedMessageContext> _delayedMessageContext;

        ThreadLocal<object> _outgoingMessage;
        Type _messageType;
        SagaContext _sagaContext;
        ThreadLocal<MessageContext> _incomingMessageContext;

        public ChainExecutionContext()
        {
            _delayedMessageContext = new ThreadLocal<DelayedMessageContext>(() => new DelayedMessageContext());
        }

        protected ChainExecutionContext(ChainExecutionContext parentContext = null): this()
        {
            _parentContext = parentContext != null ? new ThreadLocal<ChainExecutionContext>(() => parentContext) : null;
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
            get
            {
                if (_outgoingMessage != null) { return _outgoingMessage.Value; }
                else if (_parentContext != null)
                {
                    var parentContextValue = _parentContext.Value;
                    if (parentContextValue == null) { return parentContextValue; }
                    else
                    { return parentContextValue.OutgoingMessage; }
                }
                return _outgoingMessage ?? (_parentContext.IsValueCreated ? null : _parentContext.Value.OutgoingMessage);
            }
            set { _outgoingMessage = new ThreadLocal<object>(() => value); }
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

        public IChainExecutionContext Push(Action<IChainExecutionContext> populator)
        {
            var chainExecutionContext = new ChainExecutionContext(this);
            populator(chainExecutionContext);
            return chainExecutionContext;
        }

        public IEnumerable<TransportMessage> GetDelayedMessages(ChainExecutionContext context = null)
        {
            if (context == null) { context = this; }

            if (context._parentContext != null)
                foreach (var transportMessage in GetDelayedMessages(context._parentContext.Value)) yield return transportMessage;
            else
                while (context.DelayedMessageContext.DelayedMessages.Any()) yield return context.DelayedMessageContext.DelayedMessages.Dequeue();
        }

        public void Dispose()
        {
        }
    }
}