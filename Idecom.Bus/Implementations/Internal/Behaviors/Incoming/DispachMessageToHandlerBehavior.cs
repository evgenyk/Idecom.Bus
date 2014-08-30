namespace Idecom.Bus.Implementations.Internal.Behaviors.Incoming
{
    using System;
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

        public void Execute(Action next, ChainExecutionContext context)
        {
            var method = context.HandlerMethod;
            var handler = _container.Resolve(method.DeclaringType);


            var inSaga = handler is ISaga && context.SagaContext != null;
            if (inSaga)
                handler.GetType().GetProperty("Data").SetValue(handler, context.SagaContext.SagaState.SagaData);

            try
            {
                method.Invoke(handler, new[]
                                       {
                                           context.IncomingTransportMessage.Message
                                       });
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
            if (inSaga && (handler as ISaga).IsClosed)
                context.SagaContext.HandlerClosedSaga = true;

            next();
        }

    }
}