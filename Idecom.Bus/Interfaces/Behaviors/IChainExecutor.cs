namespace Idecom.Bus.Interfaces.Behaviors
{
    using System;
    using Transport;

    public interface IChainExecutor
    {
        void RunWithIt(IBehaviorChain chain, IChainExecutionContext context);
    }

    public interface IChainExecutionContext
    {
        object OutgoingMessage { get; set; }
        Type MessageType { get; set; }
        TransportMessage IncomingTransportMessage { get; set; }
    }

    public class ChainExecutionContext : IChainExecutionContext
    {
        public object OutgoingMessage { get; set; }
        public Type MessageType { get; set; }
        public TransportMessage IncomingTransportMessage { get; set; }
    }
}