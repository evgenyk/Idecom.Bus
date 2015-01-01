namespace Idecom.Bus.Testing.TestingInfrustructure
{
    using System;
    using System.Linq;
    using Implementations.Behaviors;
    using Interfaces.Behaviors;
    using Interfaces.Telemetry;

    public class IncomingTransportMessageTraceBehavior : IBehavior
    {
        readonly TestBus _testBus;

        public IncomingTransportMessageTraceBehavior(TestBus testBus)
        {
            _testBus = testBus;
        }

        public void Execute(Action next, IChainExecutionContext context)
        {
            next();
            var handlersAndMessages = context.Telemetry.Snaps.OfType<IHaveHandler>().OfType<IHaveIncomingMessageType>();

            foreach (var handlersAndMessage in handlersAndMessages)
            {
                var messageTelemetry = new MessageWithTelemetry(handlersAndMessage.IncomingMessageType, ((IHaveHandler) handlersAndMessage).Handler);
                _testBus.Snapshot.Push(messageTelemetry);
            }

        }
    }
}