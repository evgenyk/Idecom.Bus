namespace Idecom.Bus.Testing.TestingInfrustructure
{
    using System;

    public interface IMessageWithTelemetry
    {
        Type MessageType { get; }
        object Handler { get; }
        DateTime TimeReceived { get; }
    }
}