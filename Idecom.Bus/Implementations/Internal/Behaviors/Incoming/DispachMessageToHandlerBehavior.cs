namespace Idecom.Bus.Implementations.Internal.Behaviors.Incoming
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Addressing;
    using Interfaces;
    using Interfaces.Addons.Sagas;
    using Interfaces.Behaviors;
    using Interfaces.Logging;
    using Telemetry.Snaps;

    public class DispachMessageToHandlerBehavior : IBehavior
    {
        readonly IContainer _container;
        readonly ILog _log;

        public DispachMessageToHandlerBehavior(IContainer container, ILogFactory logFactory, Address address)
        {
            _log = logFactory.GetLogger(string.Format("{0} DispachMessageToHandlerBehavior", address));
            _container = container;
        }

        public void Execute(Action next, IChainExecutionContext context)
        {


            var method = context.HandlerMethod;
            var handlerType = method.DeclaringType;
            var handler = _container.Resolve(handlerType);

            var isInvalidSaga = handler is ISaga && context.SagaContext == null;

            if (isInvalidSaga)
            {
                _log.Debug(string.Format("Handler {0} for message {1} seemd to be part of a saga, but saga data was null, so skipping", handlerType.FullName, context.IncomingMessageContext.IncommingMessageType.FullName));
                next();
                return;
            }

            var inSaga = handler is ISaga && context.SagaContext != null;
            if (inSaga)
                handler.GetType().GetProperty("Data").SetValue(handler, context.SagaContext.SagaState.SagaData);

            var methodToCall = Expression.Parameter(typeof(object));
            var handledMessage = Expression.Parameter(typeof(object));

            var lambda = Expression.Lambda<Action<object, object>>(Expression.Call(Expression.Convert(methodToCall, handlerType), method, Expression.Convert(handledMessage, method.GetParameters().First().ParameterType)), methodToCall, handledMessage)
                .Compile();

            using (context.Telemetry.RecordStart(new HandlerInvocation(handler, context.IncomingMessageContext.IncommingMessageType)))
            {
                lambda(handler, context.IncomingMessageContext.IncommingMessage);
            }


            if (inSaga && (handler as ISaga).IsClosed)
                context.SagaContext.HandlerClosedSaga = true;


            next();
        }
    }
}