namespace Idecom.Bus.Implementations.Telemetry
{
    using System;
    using Interfaces.Telemetry;

    public abstract class TelemetrySnapBase : ITelemetrySnap
    {
        readonly DateTime _timeStarted;
        long _msTaken;

        protected TelemetrySnapBase()
        {
            _timeStarted = DateTime.Now;
        }

        public ITelemetrySnap RecordTime(long msTaken)
        {
            _msTaken = msTaken;
            return this;
        }

    }
}