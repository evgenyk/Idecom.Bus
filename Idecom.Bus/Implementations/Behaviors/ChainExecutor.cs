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

        protected void ExecuteNext(Queue<Type> behaviorQueue, ChainExecutionContext context)
        {
            if (!behaviorQueue.Any()) { return; }
            
            IBehavior behavior = null;
            try
            {
                behavior = ExecuteNextBehavior(_container, behaviorQueue, context);
            }
            finally {
                _container.Release(behavior);
            }
        }

        IBehavior ExecuteNextBehavior(IContainer container, Queue<Type> behaviorQueue, ChainExecutionContext context)
        {
            var nextType = behaviorQueue.Dequeue();
            var behavior = container.Resolve(nextType) as IBehavior;
            if (behavior != null) behavior.Execute(() => ExecuteNext(behaviorQueue, context), context);
            return behavior;
        }

        public virtual void RunWithIt(IBehaviorChain chain, ChainExecutionContext context)
        {
            using (_container.BeginUnitOfWork())
            {
                PopulateCurrentExecutionContexts(context);

                var behaviorQueue = new Queue<Type>(chain);
                IBehavior behavior = null;
                try
                {
                    behavior = ExecuteNextBehavior(_container, behaviorQueue, context);
                }
                finally { _container.Release(behavior); }
            }
        }

        void PopulateCurrentExecutionContexts(ChainExecutionContext context)
        {
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