namespace Idecom.Bus.Implementations.Behaviors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Interfaces;
    using Interfaces.Behaviors;

    public class ChainExecutor : IChainExecutor
    {
        readonly IContainer _container;

        public ChainExecutor(IContainer container)
        {
            _container = container;
        }

        public virtual void RunWithIt(IBehaviorChain chain, IChainExecutionContext context)
        {
            using (_container.BeginUnitOfWork())
            {
                var behaviorQueue = new Queue<Type>(chain);
                IBehavior behavior = null;
                try { behavior = ExecuteNextBehavior(_container, behaviorQueue, context); }
                finally { _container.Release(behavior); }
            }
        }

        IBehavior ExecuteNextBehavior(IContainer container, Queue<Type> behaviorQueue, IChainExecutionContext context)
        {
            var nextType = behaviorQueue.Dequeue();

            var behavior = container.Resolve(nextType) as IBehavior;

            if (behavior != null)
                behavior.Execute(() =>
                                 {
                                     if (!behaviorQueue.Any()) { return; }

                                     IBehavior behavior1 = null;
                                     try { behavior1 = ExecuteNextBehavior(_container, behaviorQueue, context); }
                                     finally { _container.Release(behavior1); }
                                 }, context);
            return behavior;
        }
    }
}