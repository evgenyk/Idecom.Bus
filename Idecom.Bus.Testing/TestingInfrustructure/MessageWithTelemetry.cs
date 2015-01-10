namespace Idecom.Bus.Testing.TestingInfrustructure
{
    using System;


    public class MessageWithTelemetry : IMessageWithTelemetry
    {
        public MessageWithTelemetry(Type messageType, object handler)
        {
            MessageType = messageType;
            Handler = handler;
            TimeReceived = DateTime.Now;
        }

        public Type MessageType { get; private set; }
        public object Handler { get; private set; }

        public DateTime TimeReceived { get; private set; }
    }
}