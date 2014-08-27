namespace Idecom.Bus.Implementations.Internal.Behaviors.Incoming
{
    using System;
    using System.Linq;
    using Interfaces;
    using Interfaces.Addons.Sagas;
    using Interfaces.Behaviors;
    using UnicastBus;

    public class ResumeSagaBehavior: IBehavior
    {
        readonly MessageContext _messageContext;
        readonly IContainer _container;
        readonly HandlerContext _handlerContext;
        readonly IMessageToStartSagaMapping _messageToStartSagaMapping;
        readonly ISagaManager _sagaManager;

        public ResumeSagaBehavior(MessageContext messageContext, IContainer container, HandlerContext handlerContext, IMessageToStartSagaMapping messageToStartSagaMapping, ISagaManager sagaManager)
        {
            _messageContext = messageContext;
            _container = container;
            _handlerContext = handlerContext;
            _messageToStartSagaMapping = messageToStartSagaMapping;
            _sagaManager = sagaManager;
        }

        public void Execute(Action next, ChainExecutionContext context)
        {
            var handlerMethod = _handlerContext.Method;
            if (IsSubclassOfRawGeneric(typeof (Saga<>), handlerMethod.DeclaringType)) //this must be a saga, whether existing or a new one is a diffirent question
            {
                var sagaDataType = handlerMethod.DeclaringType.BaseType.GenericTypeArguments.First();
                var startSagaTypes = _messageToStartSagaMapping.ResolveRouteFor(_messageContext.IncomingTransportMessage.MessageType ?? _messageContext.IncomingTransportMessage.GetType());
                ISagaStateInstance sagaData;
                if (startSagaTypes != null) sagaData = _sagaManager.Start(sagaDataType, _messageContext);
                else sagaData = _sagaManager.Resume(sagaDataType, _messageContext);

                throw new NotImplementedException("Figure out how to assign SagaData to a Handler here now");

            }

            next();
        }


        static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur) { return true; }
                toCheck = toCheck.BaseType;
            }
            return false;
        }

    }
}