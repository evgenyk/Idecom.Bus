﻿namespace Idecom.Bus.Implementations.Internal.Behaviors.Incoming
{
    using System;
    using System.Linq;
    using Interfaces;
    using Interfaces.Addons.Sagas;
    using Interfaces.Behaviors;
    using UnicastBus;

    public class ResumeSagaBehavior : IBehavior
    {
        readonly IMessageToStartSagaMapping _messageToStartSagaMapping;
        readonly ISagaManager _sagaManager;

        public ResumeSagaBehavior(IMessageToStartSagaMapping messageToStartSagaMapping, ISagaManager sagaManager)
        {
            _messageToStartSagaMapping = messageToStartSagaMapping;
            _sagaManager = sagaManager;
        }

        public void Execute(Action next, IChainExecutionContext context)
        {
            var handlerMethod = context.HandlerMethod;
            if (IsSubclassOfRawGeneric(typeof (Saga<>), handlerMethod.DeclaringType)) //this must be a saga, whether existing or a new one is a diffirent question
            {
                var sagaDataType = handlerMethod.DeclaringType.BaseType.GenericTypeArguments.First();
                var startSagaTypes = _messageToStartSagaMapping.ResolveRouteFor(context.IncomingMessageContext.IncommingMessageType);
                ISagaStateInstance sagaData;
                if (startSagaTypes != null) sagaData = _sagaManager.Start(sagaDataType, context.OutgoingHeaders);
                else
                {
                    sagaData = _sagaManager.Resume(sagaDataType, context.IncomingMessageContext);
                    if (sagaData == null)
                    {
                        var sagaId = "no saga id present in incoming message headers";
                        if (context.IncomingMessageContext.ContainsSagaIdForType(sagaDataType))
                            sagaId = context.IncomingMessageContext.GetSagaIdForType(sagaDataType);
                        Console.WriteLine("Could not find saga data for message type: {0}, sagaId: {1}", context.IncomingMessageContext.IncommingMessageType, sagaId);
                        return;
                    }
                }

                if (sagaData != null) { context.SagaContext = new SagaContext {SagaState = sagaData}; }

                next();

                if (sagaData == null) return;

                if (context.SagaContext.HandlerClosedSaga) { _sagaManager.CloseSaga(sagaData); }
                else _sagaManager.UpdateSaga(sagaData);
            }
            else
            { next(); }
        }


        static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof (object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur) { return true; }
                toCheck = toCheck.BaseType;
            }
            return false;
        }
    }
}