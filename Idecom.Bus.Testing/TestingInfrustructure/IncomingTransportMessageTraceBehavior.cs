namespace Idecom.Bus.Testing.TestingInfrustructure
{
    using System;
    using Interfaces.Behaviors;

    public class IncomingTransportMessageTraceBehavior : IBehavior
    {
        readonly TestBus _testBus;

        public IncomingTransportMessageTraceBehavior(TestBus testBus)
        {
            _testBus = testBus;
        }

        public void Execute(Action next, IChainExecutionContext context)
        {
            _testBus.MessagesReceived.Add(context.IncomingMessageContext.IncommingMessage);
            next();
        }
    }
}