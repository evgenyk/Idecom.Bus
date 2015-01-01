namespace Idecom.Bus.Implementations.Telemetry
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Interfaces.Telemetry;

    class ChainTelemetry : IChainTelemetry
    {
        readonly ConcurrentDictionary<Guid, ITelemetrySnap> _recordings = new ConcurrentDictionary<Guid, ITelemetrySnap>();
        public ITelemetryToken RecordStart<T>(T parameters) where T : ITelemetrySnap
        {
            var id = Guid.NewGuid();
            _recordings.AddOrUpdate(id, guid => parameters, (guid, snap) => snap);
            return new TelemetryToken(millisecondsTook => _recordings.AddOrUpdate(id, guid => parameters, (guid, snap) => snap.RecordTime(millisecondsTook)));
        }

        public IEnumerable<ITelemetrySnap> Snaps
        {
            get { return _recordings.Values; }
        }
    }
}