namespace Idecom.Bus.Interfaces.Telemetry
{
    public interface ITelemetrySnap
    {
        ITelemetrySnap RecordTime(long msTaken);
    }
}