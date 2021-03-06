﻿namespace Idecom.Bus.Transport
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Addressing;

    [DebuggerStepThrough, Serializable]
    public class TransportMessage
    {
        public TransportMessage(object message, Address sourceAddress, Address targetAddress, MessageIntent intent, Type messageType, Dictionary<string, string> headers = null)
            : this(message, headers)
        {
            if (intent != MessageIntent.Publish && targetAddress == null)
                throw new ArgumentNullException("targetAddress");

            SourceAddress = sourceAddress;
            TargetAddress = targetAddress;
            Intent = intent;
            MessageType = messageType;
        }

        TransportMessage(object message, Dictionary<string, string> headers)
        {
            Message = message;
            Headers = new Dictionary<string, string>(headers ?? new Dictionary<string, string>());
        }

        public Address SourceAddress { get; private set; }
        public Address TargetAddress { get; private set; }
        public MessageIntent Intent { get; private set; }
        public Type MessageType { get; private set; }
        public object Message { get; private set; }
        public Dictionary<string, string> Headers { get; private set; }
    }
}