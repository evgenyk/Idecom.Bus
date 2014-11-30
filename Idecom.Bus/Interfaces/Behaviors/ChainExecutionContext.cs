namespace Idecom.Bus.Interfaces.Behaviors
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Remoting.Messaging;
    using Implementations.UnicastBus;
    using Transport;
    using Utility;

    public interface IChainExecutionContext : IDisposable
    {
        IncommingMessageContext IncomingMessageContext { get; set; }
        SagaContext SagaContext { get; set; }
        object OutgoingMessage { get; set; }
        Type OutgoingMessageType { get; set; }
        MethodInfo HandlerMethod { get; set; }
        ConcurrentDictionary<string, string> OutgoingHeaders { get; }
        IChainExecutionContext Push(Action<IChainExecutionContext> populator);
        void DelayMessage(TransportMessage transportMessage, ChainExecutionContext context = null);
        IEnumerable<TransportMessage> GetDelayedMessages(ChainExecutionContext context = null);
        bool IsProcessingIncomingMessage();
    }

    public class ChainExecutionContext : IChainExecutionContext
    {
        readonly DelayedMessageContext _delayedMessageContext;
        readonly ConcurrentDictionary<string, string> _outgoingHeaders;
        readonly ChainExecutionContext _parentContext;
        IncommingMessageContext _incomingMessageContext;
        object _outgoingMessage;
        Type _outgoingMessageType;
        SagaContext _sagaContext;


        internal ChainExecutionContext(ChainExecutionContext parentContext = null)
        {
            _parentContext = parentContext;
            if (_parentContext != null)
                return;

            // those things leave only in the top-most context for misterious reasons you'd understand when read code more closely
            _delayedMessageContext = new DelayedMessageContext();
            _outgoingHeaders = new ConcurrentDictionary<string, string>();
        }

        public DelayedMessageContext DelayedMessageContext
        {
            get { return _delayedMessageContext ?? (_parentContext == null ? null : _parentContext.DelayedMessageContext); }
        }

        public ConcurrentDictionary<string, string> OutgoingHeaders
        {
            get
            {
                if (_parentContext != null) { return _parentContext.OutgoingHeaders; }
                return _outgoingHeaders;
            }
        }

        public SagaContext SagaContext
        {
            get
            {
                if (_sagaContext != null)
                    return _sagaContext;
                return _parentContext == null ? null : _parentContext.SagaContext;
            }
            set
            {
                if (_parentContext == null)
                    _sagaContext = value;
                else
                { _parentContext.SagaContext = value; }
            }
        }

        public object OutgoingMessage
        {
            get
            {
                if (_outgoingMessage != null) { return _outgoingMessage; }

                if (_parentContext == null)
                    return _outgoingMessage;
                var parentContextValue = _parentContext;
                return parentContextValue == null ? _outgoingMessage : parentContextValue.OutgoingMessage;
            }
            set { _outgoingMessage = value; }
        }

        public Type OutgoingMessageType
        {
            get
            {
                if (_outgoingMessageType != null) { return _outgoingMessageType; }

                if (_parentContext == null)
                    return _outgoingMessageType;
                var parentContextValue = _parentContext;
                return parentContextValue == null ? _outgoingMessageType : parentContextValue.OutgoingMessageType;
            }
            set { _outgoingMessageType = value; }
        }

        public MethodInfo HandlerMethod { get; set; } //handler method can not be inherited as it's always local

        public void DelayMessage(TransportMessage transportMessage, ChainExecutionContext context = null)
        {
            if (context == null) { context = this; }

            if (context._parentContext != null)
                DelayMessage(transportMessage, context._parentContext);
            else context.DelayedMessageContext.Enqueue(transportMessage);
        }

        public IncommingMessageContext IncomingMessageContext
        {
            get
            {
                return _incomingMessageContext ?? (_parentContext != null ? _parentContext.IncomingMessageContext : null);
            }
            set
            {
                if (_parentContext == null) { _incomingMessageContext = value; }
                else
                { _parentContext.IncomingMessageContext = value; }
            }
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
                foreach (var transportMessage in GetDelayedMessages(context._parentContext)) yield return transportMessage;
            else
                while (context.DelayedMessageContext.DelayedMessages.Any()) yield return context.DelayedMessageContext.DelayedMessages.Dequeue();
        }

        public bool IsProcessingIncomingMessage()
        {
            return IncomingMessageContext != null;
        }

        public void Dispose()
        {
            if (_parentContext == null)
                CallContext.FreeNamedDataSlot(SystemHeaders.CallContext.AmbientContext);
            else _parentContext.Dispose();
        }
    }
}