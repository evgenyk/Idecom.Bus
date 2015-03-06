namespace Idecom.Bus.Interfaces
{
    using System;
    using System.Collections.Generic;

    public interface IDebugView
    {
        void RecordEventsWithHandlers(IEnumerable<Type> eventsWithHandlers);
        void RecordEventsDiscovered(IEnumerable<Type> events);
        void RecordCommandsDiscovered(IEnumerable<Type> commands);
        void RecordBeforeStarted(List<IBeforeBusStarted> beforeBusStarteds);
    }
}