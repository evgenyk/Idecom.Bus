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

        protected void ExecuteNext(IContainer container, Queue<Type> behaviorQueue)
        {
            if (!behaviorQueue.Any()) { return; }
            var nextType = behaviorQueue.Dequeue();
            
            IBehavior behavior = null;
            try
            {
                behavior = container.Resolve(nextType) as IBehavior;
                if (behavior != null) behavior.Execute(() => ExecuteNext(container, behaviorQueue));
            }
            finally {
                _container.Release(behavior);
            }
        }

        public virtual void RunWithIt(BehaviorChain chain)
        {
            var behaviorQueue = new Queue<Type>(chain);
            IBehavior behavior = null;
            try
            {
                var typeToBuild = behaviorQueue.Dequeue();
                behavior = _container.Resolve(typeToBuild) as IBehavior;
                if (behavior != null) behavior.Execute(() => ExecuteNext(_container, behaviorQueue));
            }
            finally {
                _container.Release(behavior);
            }
        }
    }
}