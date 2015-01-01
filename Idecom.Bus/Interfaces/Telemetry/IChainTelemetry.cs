namespace Idecom.Bus.Interfaces.Telemetry
{
    using System.Collections.Generic;

    public interface IChainTelemetry
    {
        ITelemetryToken RecordStart<T>(T parameters) where T: ITelemetrySnap;
        IEnumerable<ITelemetrySnap> Snaps { get; }
    }
}