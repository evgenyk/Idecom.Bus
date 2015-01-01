namespace Idecom.Bus.Implementations.Internal.Behaviors.Incoming
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Implementations.Behaviors;
    using Interfaces;
    using Interfaces.Addons.Sagas;
    using Interfaces.Behaviors;
    using Telemetry;
    using Telemetry.Snaps;

    public class DispachMessageToHandlerBehavior : IBehavior
    {
        readonly IContainer _container;

        public DispachMessageToHandlerBehavior(IContainer container)
        {
            _container = container;
        }

        public void Execute(Action next, IChainExecutionContext context)
        {


            var method = context.HandlerMethod;
            var handlerType = method.DeclaringType;
            var handler = _container.Resolve(handlerType);

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