namespace Idecom.Bus.Testing.TestingInfrustructure
{
    using System;

    public class MessageWithTelemetry : IMessageWithTelemetry
    {
        public MessageWithTelemetry(object message)
        {
            Message = message;
            TimeReceived = DateTime.Now;
        }

        public object Message { get; private set; }

        public DateTime TimeReceived { get; private set; }
    }
}