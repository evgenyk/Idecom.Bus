namespace Idecom.Bus.Testing.TestingInfrustructure
{
    using System;

    public interface IMessageWithTelemetry
    {
        object Message { get; }
        DateTime TimeReceived { get; }
    }
}