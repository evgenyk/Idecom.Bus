namespace Idecom.Bus.Interfaces.Telemetry
{
    using System;

    public interface ITelemetryToken: IDisposable
    {
        void Resolve();
    }
}