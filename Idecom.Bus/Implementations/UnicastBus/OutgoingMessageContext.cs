namespace Idecom.Bus.Implementations.UnicastBus
{
    using System;

    public class OutgoingMessageContext
    {
        public object Message { get; set; }
        public Type MessageType { get; set; }
    }
}