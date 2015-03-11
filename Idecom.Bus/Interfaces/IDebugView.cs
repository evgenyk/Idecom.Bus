namespace Idecom.Bus.Interfaces
{
    using System;
    using System.Collections.Generic;
    using Implementations;

    public interface IDebugView
    {
        IList<Type> EventsDiscovered { get; set; }
        IList<Type> CommandsDiscovered { get; set; }
        IList<Type> EventsWithHandlers { get; set; }
        IList<IBeforeBusStarted> BeforeStartedDiscovered { get; set; }
        List<ClassToMessageInfo> Handlers { get; set; }
        List<ClassToMessageInfo> Sagas { get; set; }
        void RecordEventsWithHandlers(IEnumerable<Type> eventsWithHandlers);
        void RecordEventsDiscovered(IEnumerable<Type> events);
        void RecordCommandsDiscovered(IEnumerable<Type> commands);
        void RecordBeforeStarted(List<IBeforeBusStarted> beforeBusStarteds);
        void RecordHandler(Type handlerType, Type messageType);
        void SagaStarts(Type sagaType, Type messageType);
    }
}