namespace Idecom.Bus.Implementations.Behaviors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Interfaces;
    using Interfaces.Behaviors;
    using UnicastBus;

    public class ChainExecutor : IChainExecutor
    {
        readonly IContainer _container;

        public ChainExecutor(IContainer container)
        {
            _container = container;
        }

        public virtual void RunWithIt(IBehaviorChain chain, ChainExecutionContext context)
        {
            using (_container.BeginUnitOfWork())
            {
                var behaviorQueue = new Queue<Type>(chain);
                IBehavior behavior = null;
                try { behavior = ExecuteNextBehavior(_container, behaviorQueue, context); }
                finally { _container.Release(behavior); }
            }
        }

        protected void ExecuteNext(Queue<Type> behaviorQueue, ChainExecutionContext context)
        {
            if (!behaviorQueue.Any()) { return; }

            IBehavior behavior = null;
            try { behavior = ExecuteNextBehavior(_container, behaviorQueue, context); }
            finally { _container.Release(behavior); }
        }

        IBehavior ExecuteNextBehavior(IContainer container, Queue<Type> behaviorQueue, ChainExecutionContext context)
        {
            var nextType = behaviorQueue.Dequeue();
            IBehavior behavior = null;
            try {
                behavior = container.Resolve(nextType) as IBehavior;
            }
            catch (Exception e) {
                throw;
            }
            if (behavior != null)
            {
                behavior.Execute(() => ExecuteNext(behaviorQueue, context), context);
            }
            return behavior;
        }

    }
}