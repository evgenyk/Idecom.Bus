namespace Idecom.Bus.Implementations.Internal.Behaviors
{
    using System;
    using Interfaces;
    using Interfaces.Behaviors;

    public class ThrowIfBusNotStartedBehavior : IBehavior
    {
        readonly IBusInstance _bus;

        public ThrowIfBusNotStartedBehavior(IBusInstance bus)
        {
            _bus = bus;
        }

        public void Execute(Action next)
        {
            if (!_bus.IsStarted) { throw new Exception("Bus should be strated to process any messages"); }
            next();
        }
    }
}