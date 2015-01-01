namespace Idecom.Bus.Implementations.Telemetry
{
    using System;
    using System.Diagnostics;
    using Interfaces.Telemetry;

    class TelemetryToken : ITelemetryToken
    {
        readonly Action<long> _action;
        readonly Stopwatch _sw;

        public TelemetryToken(Action<long> action)
        {
            _action = action;
            _sw = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            Resolve();
        }

        public void Resolve()
        {
            _action(_sw.ElapsedMilliseconds);
        }
    }
}