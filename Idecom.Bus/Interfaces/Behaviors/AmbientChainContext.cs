namespace Idecom.Bus.Interfaces.Behaviors
{
    using System.Threading;

    public static class AmbientChainContext
    {
        static ThreadLocal<ChainExecutionContext> _current;

        public static ChainExecutionContext Current
        {
            get
            {
                if (_current == null) { _current = new ThreadLocal<ChainExecutionContext>(() => new ChainExecutionContext()); }
                return _current.Value;
            }
        }
    }
}