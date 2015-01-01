namespace Idecom.Bus.Interfaces.Telemetry
{
    using System;

    public interface IHaveIncomingMessageType
    {
        Type IncomingMessageType { get; }
    }
}