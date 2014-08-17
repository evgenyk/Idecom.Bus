namespace Idecom.Bus.Interfaces.Behaviors
{
    using Transport;

    public interface IChainExecutor
    {
        void RunWithIt(IBehaviorChain chain, IChainExecutionContext context);
    }

    public interface IChainExecutionContext
    {
        object OutgoingMessage { get; set; }
        MessageIntent Intent { get; set; }
    }

    public class ChainExecutionContext : IChainExecutionContext
    {
        public object OutgoingMessage { get; set; }
        public MessageIntent Intent { get; set; }
    }
}