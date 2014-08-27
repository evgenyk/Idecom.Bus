namespace Idecom.Bus.Interfaces.Behaviors
{
    using System;
    using System.Reflection;
    using Transport;

    public class ChainExecutionContext
    {
        readonly ChainExecutionContext _parentContext;
        object _outgoingMessage;
        Type _messageType;
        TransportMessage _incomingTransportMessage;
        MethodInfo _handlerMethod;

        public ChainExecutionContext(ChainExecutionContext parentContext = null)
        {
            _parentContext = parentContext;
        }

        public object OutgoingMessage
        {
            get { return _outgoingMessage ?? (_parentContext == null ? null : _parentContext.OutgoingMessage); }
            set { _outgoingMessage = value; }
        }

        public Type MessageType
        {
            get { return _messageType ?? (_parentContext == null ? null : _parentContext.MessageType); }
            set { _messageType = value; }
        }

        public TransportMessage IncomingTransportMessage
        {
            get { return _incomingTransportMessage ?? (_parentContext == null ? null :_parentContext.IncomingTransportMessage); }
            set { _incomingTransportMessage = value; }
        }

        public MethodInfo HandlerMethod
        {
            get { return _handlerMethod ?? (_parentContext == null ? null : _parentContext.HandlerMethod); }
            set { _handlerMethod = value; }
        }
    }
}