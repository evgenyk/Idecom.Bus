namespace Idecom.Bus.Interfaces.Behaviors
{
    using System.Threading;

    public static class AmbientChainContext
    {
        static ThreadLocal<ChainExecutionContext> _current;

        /// <summary>
        /// This thing is using ThreadLocal so make sure you're not re-using threads as this might mess things up
        /// </summary>
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