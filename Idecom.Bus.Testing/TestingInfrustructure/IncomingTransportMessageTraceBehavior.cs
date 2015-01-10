namespace Idecom.Bus.Testing.TestingInfrustructure
{
    using System;
    using System.Linq;
    using Castle.Core.Internal;
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

            context.Telemetry.Snaps.OfType<IHaveBehavior>().ForEach(b=>_testBus.Snapshot.Push(new BehaviorInvocation(b.Behavior)));

            foreach (var handlersAndMessage in handlersAndMessages)
            {
                var messageTelemetry = new MessageWithTelemetry(handlersAndMessage.IncomingMessageType, ((IHaveHandler) handlersAndMessage).Handler);
                _testBus.Snapshot.Push(messageTelemetry);
            }

        }
    }
}