namespace Idecom.Bus.Implementations.Behaviors
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Interfaces;
    using Interfaces.Behaviors;
    using UnicastBus;

    [DebuggerNonUserCode]
    public class ChainExecutor : IChainExecutor
    {
        readonly IContainer _container;

        public ChainExecutor(IContainer container)
        {
            _container = container;
        }

        protected void ExecuteNext(Queue<Type> behaviorQueue)
        {
            if (!behaviorQueue.Any()) { return; }
            
            IBehavior behavior = null;
            try
            {
                behavior = ExecuteNextBehavior(_container, behaviorQueue);
            }
            finally {
                _container.Release(behavior);
            }
        }

        IBehavior ExecuteNextBehavior(IContainer container, Queue<Type> behaviorQueue)
        {
            var nextType = behaviorQueue.Dequeue();
            var behavior = container.Resolve(nextType) as IBehavior;
            if (behavior != null) behavior.Execute(() => ExecuteNext(behaviorQueue));
            return behavior;
        }

        public virtual void RunWithIt(IBehaviorChain chain, IChainExecutionContext context)
        {
            using (_container.BeginUnitOfWork())
            {
                PopulateCurrentExecutionContexts(context);

                var behaviorQueue = new Queue<Type>(chain);
                IBehavior behavior = null;
                try
                {
                    behavior = ExecuteNextBehavior(_container, behaviorQueue);
                }
                finally { _container.Release(behavior); }
            }
        }

        void PopulateCurrentExecutionContexts(IChainExecutionContext context)
        {
            //so far it's the only way I couild maintain a nice interface for behaviors
            if (context.OutgoingMessage != null)
            {
                var outgoingMessageContext = _container.Resolve<OutgoingMessageContext>();
                outgoingMessageContext.Message = context.OutgoingMessage;
                outgoingMessageContext.MessageType = context.MessageType ?? context.OutgoingMessage.GetType();
            }


            if (context.IncomingTransportMessage != null)
            {
                var incomingMessageContext = _container.Resolve<MessageContext>();
                incomingMessageContext.IncomingTransportMessage = context.IncomingTransportMessage;
            }

            if (context.HandlerMethod != null) {
                var handlerContext = _container.Resolve<HandlerContext>();
                handlerContext.Method = context.HandlerMethod;
            }
        }
    }
}