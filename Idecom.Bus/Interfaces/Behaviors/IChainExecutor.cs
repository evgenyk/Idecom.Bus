namespace Idecom.Bus.Interfaces.Behaviors
{
    using Transport;

    public interface IChainExecutor
    {
        void RunWithIt(IBehaviorChain chain, IChainExecutionContext context);
    }

    public interface IChainExecutionContext
    {
        TransportMessage OutgoingMessage { get; set; }
    }

    public class ChainExecutionContext : IChainExecutionContext
    {
        public TransportMessage OutgoingMessage { get; set; }
    }
}