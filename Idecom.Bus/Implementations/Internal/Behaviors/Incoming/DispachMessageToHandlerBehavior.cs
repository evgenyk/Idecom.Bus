namespace Idecom.Bus.Implementations.Internal.Behaviors.Incoming
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Interfaces;
    using Interfaces.Addons.Sagas;
    using Interfaces.Behaviors;

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
            var handler = _container.Resolve(method.DeclaringType);

            var inSaga = handler is ISaga && context.SagaContext != null;
            if (inSaga)
                handler.GetType().GetProperty("Data").SetValue(handler, context.SagaContext.SagaState.SagaData);

            var methodToCall = Expression.Parameter(typeof(object));
            var handledMessage = Expression.Parameter(typeof(object));

            var lambda = Expression.Lambda<Action<object, object>>(Expression.Call(Expression.Convert(methodToCall, method.DeclaringType), method, Expression.Convert(handledMessage, method.GetParameters().First().ParameterType)), methodToCall, handledMessage).Compile();
            lambda(handler, context.IncomingMessageContext.IncomingTransportMessage.Message);

            if (inSaga && (handler as ISaga).IsClosed)
                context.SagaContext.HandlerClosedSaga = true;

            next();
        }

    }
}