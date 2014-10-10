namespace Idecom.Bus.Implementations.Internal.Behaviors.Incoming
{
    using System;
    using System.Linq;
    using Interfaces;
    using Interfaces.Addons.Sagas;
    using Interfaces.Behaviors;
    using Interfaces.Logging;
    using UnicastBus;

    public class ResumeSagaBehavior : IBehavior
    {
        readonly IMessageToStartSagaMapping _messageToStartSagaMapping;
        readonly ISagaManager _sagaManager;
        public ILog Log { get; set; }

        public ResumeSagaBehavior(IMessageToStartSagaMapping messageToStartSagaMapping, ISagaManager sagaManager)
        {
            _messageToStartSagaMapping = messageToStartSagaMapping;
            _sagaManager = sagaManager;
        }

        public void Execute(Action next, IChainExecutionContext context)
        {
            var handlerMethod = context.HandlerMethod;
            if (IsSubclassOfRawGeneric(typeof(Saga<>), handlerMethod.DeclaringType)) //this must be a saga, whether existing or a new one is a diffirent question
            {
                var sagaDataType = handlerMethod.DeclaringType.BaseType.GenericTypeArguments.First();
                var startSagaTypes = _messageToStartSagaMapping.ResolveRouteFor(context.IncomingMessageContext.IncommingMessageType);
                ISagaStateInstance sagaInstance;

                if (startSagaTypes != null)
                {
                    Log.Debug("Starting saga...");
                    sagaInstance = _sagaManager.Start(sagaDataType, context.OutgoingHeaders);
                }
                else
                {
                    var sagaId = "no saga id present in incoming message headers";
                    if (context.IncomingMessageContext.ContainsSagaIdForType(sagaDataType))
                        sagaId = context.IncomingMessageContext.GetSagaIdForType(sagaDataType);

                    Log.Debug("Resuming saga " + sagaId);
                    sagaInstance = _sagaManager.ResumeSaga(sagaDataType, context.IncomingMessageContext);
                    if (sagaInstance == null | (sagaInstance != null && sagaInstance.SagaData == null))
                    {
                        var message = string.Format("This could never happen under normal circumstances. Could not find saga data for message type: {0}, sagaId: {1}",
                            context.IncomingMessageContext.IncommingMessageType, sagaId);
                        
                        Log.Debug(message);
                        throw new Exception(message);
                    }
                }

                if (sagaInstance != null) { context.SagaContext = new SagaContext { SagaState = sagaInstance }; }

                next();

                if (sagaInstance == null) return;

                if (context.SagaContext.HandlerClosedSaga)
                {
                    Log.DebugFormat("Closed saga id {0}", sagaInstance.SagaId);
                    _sagaManager.CloseSaga(sagaInstance);
                }
                else _sagaManager.UpdateSaga(sagaInstance);
            }
            else
            {
                next();
            }
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