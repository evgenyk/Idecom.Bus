namespace Idecom.Bus.Interfaces.Behaviors
{
    using System.Runtime.Remoting.Messaging;
    using Utility;

    public static class AmbientChainContext
    {
        static readonly object LockRoot = new object();

        public static ChainExecutionContext Current
        {
            get
            {
                lock (LockRoot)
                {
                    var ambientContext = CallContext.LogicalGetData(SystemHeaders.CallContext.AmbientContext) as ChainExecutionContext;

                    if (ambientContext != null) return ambientContext;

                    var chainExecutionContext = new ChainExecutionContext();
                    CallContext.LogicalSetData(SystemHeaders.CallContext.AmbientContext, chainExecutionContext);
                    return chainExecutionContext;
                }
            }
        }
    }
}