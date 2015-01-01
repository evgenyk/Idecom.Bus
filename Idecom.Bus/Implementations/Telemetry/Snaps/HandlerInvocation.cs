namespace Idecom.Bus.Implementations.Telemetry.Snaps
{
    using System;
    using Interfaces.Telemetry;

    public class HandlerInvocation : TelemetrySnapBase, IHaveHandler, IHaveIncomingMessageType
    {
        public object Handler { get; private set; }

        public HandlerInvocation(object handler, Type incomingMessageType)
        {
            Handler = handler;
            IncomingMessageType = incomingMessageType;
        }

        public Type IncomingMessageType { get; private set; }
    }
}