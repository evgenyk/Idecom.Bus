namespace Idecom.Bus.Implementations.UnicastBus
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Addressing;
    using Interfaces;
    using Interfaces.Addons.PubSub;
    using Interfaces.Addons.Sagas;
    using Internal;
    using Transport;
    using Utility;

    class Bus : IBusInstance
    {
        bool _isStarted;
        public IContainer Container { get; set; }
        public IMultiRoutingTable<MethodInfo> HandlerRoutingTable { get; set; }
        public IRoutingTable<Address> MessageRoutingTable { get; set; }
        public IRoutingTable<Type> MessageToStartSagaMapping { get; set; }
        public IMessageSerializer Serializer { get; set; }
        public IInstanceCreator InstanceCreator { get; set; }
        public ISubscriptionDistributor SubscriptionDistributor { get; set; }
        public ITransport Transport { get; set; }
        public Address LocalAddress { get; set; }
        public IEffectiveConfiguration EffectiveConfiguration { get; set; }
        public ISagaStorage SagaStorage { get; set; }
        public ISagaManager SagaManager { get; set; }


        public IMessageContext CurrentMessageContext
        {
            get { return CurrentMessageContextInternal(); }
        }

        public IBusInstance Start()
        {
            lock (this)
            {
                if (_isStarted)
                    throw new ArgumentException("Can't start bus which already started.");
                if (Transport == null)
                    throw new ArgumentException("Can not create bus. Transport hasn't been provided.");
                if (Container == null)
                    throw new ArgumentException("Can not create bus. Container hasn't been provided.");
                if (Serializer == null)
                    throw new ArgumentException("Can not create bus. Message serializer hasn't been provided.");

                Container.Configure<CurrentMessageContext>(ComponentLifecycle.PerUnitOfWork);


                var allTypes = AssemblyScanner.GetTypes().ToList();
                var events = allTypes.Where(EffectiveConfiguration.IsEvent).ToList();
                var commands = allTypes.Where(EffectiveConfiguration.IsCommand).ToList();
                ApplyHandlerMapping(events, commands, allTypes);

                var eventsWithHandlers = events.Where(e => HandlerRoutingTable.ResolveRouteFor(e).Any()).ToList();


                Transport.TransportMessageReceived += TransportMessageReceived;
                Transport.TransportMessageFinished += TransportOnTransportMessageFinished;

                foreach (var beforeBusStarted in Container.ResolveAll<IBeforeBusStarted>())
                {
                    beforeBusStarted.BeforeBusStarted();
                    Container.Release(beforeBusStarted);
                }

                if (events.Any())
                    if (SubscriptionDistributor == null)
                        throw new Exception(string.Format("Could not start pub/sub infrustructure: found {0} event(s) but SubscriptionsDistributor has not been configured", events.Count));

                SubscriptionDistributor.Unsubscribe(events.Except(eventsWithHandlers));
                SubscriptionDistributor.SubscribeTo(eventsWithHandlers);


                _isStarted = true;
            }
            return this;
        }

        public void Stop()
        {
            if (!_isStarted)
                throw new Exception("Can not stop a bus that hasn't been started.");

            lock (this)
            {
                foreach (var beforeBusStopped in Container.ResolveAll<IBeforeBusStopped>())
                {
                    beforeBusStopped.BeforeBusStopped();
                    Container.Release(beforeBusStopped);
                }
                _isStarted = false;
            }
        }

        public void Send(object message)
        {
            ExecuteOnlyWhenStarted(
                () => Transport.Send(new TransportMessage(message, LocalAddress, MessageRoutingTable.ResolveRouteFor(message.GetType()), MessageIntent.Send), CurrentMessageContextInternal()));
        }

        public void SendLocal(object message)
        {
            ExecuteOnlyWhenStarted(() => Transport.Send(new TransportMessage(message, LocalAddress, LocalAddress, MessageIntent.SendLocal), CurrentMessageContextInternal()));
        }

        public void Reply(object message)
        {
            if (LocalAddress.Equals(CurrentMessageContext.TransportMessage.SourceAddress))
                throw new Exception(string.Format("Received a message with reply address as a local queue. This can cause an infinite loop and been stopped. Queue: {0}",
                    CurrentMessageContext.TransportMessage.SourceAddress));

            ExecuteOnlyWhenStarted(
                () => Transport.Send(new TransportMessage(message, LocalAddress, MessageRoutingTable.ResolveRouteFor(message.GetType()), MessageIntent.Reply), CurrentMessageContextInternal()));
        }

        public void Raise<T>(Action<T> action) where T : class
        {
            var message = InstanceCreator.CreateInstanceOf<T>();
            if (action != null)
                action(message);

            SubscriptionDistributor.NotifySubscribersOf<T>(message, CurrentMessageContextInternal());
        }

        [DebuggerStepThrough]
        CurrentMessageContext CurrentMessageContextInternal()
        {
            var currentMessageContext = Container.Resolve<CurrentMessageContext>();
            Container.Release(currentMessageContext);
            return currentMessageContext;
        }

        [DebuggerStepThrough]
        void ExecuteOnlyWhenStarted(Action todo)
        {
            if (_isStarted || CurrentMessageContext != null)
                todo();
            else
                throw new Exception("Can not send or receive messages while the bus is stopped.");
        }

        void TransportMessageReceived(object sender, TransportMessageReceivedEventArgs e)
        {
            CurrentMessageContext currentMessageContext = null;
            try
            {
                currentMessageContext = Container.Resolve<CurrentMessageContext>();
                if (currentMessageContext == null)
                    throw new Exception("Could not resolve current message context");

                currentMessageContext.TransportMessage = e.TransportMessage;
                currentMessageContext.Attempt = e.Attempt;
                currentMessageContext.MaxAttempts = e.MaxRetries + 1;

                var message = e.TransportMessage.Message;
                var type = e.TransportMessage.MessageType ?? message.GetType();
                var handlerMethods = HandlerRoutingTable.ResolveRouteFor(type);

                var executedHandlers = handlerMethods.Select(handler => ExecuteHandler(message, type, handler, currentMessageContext)).ToList();

                if (executedHandlers.All(x => !x))
                    Console.WriteLine("Warning: {1} received a message of type {0}, but could not find a handler for it", e.TransportMessage.MessageType, LocalAddress);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while receiving a message: " + ex);
                throw;
            }
            finally {
                Container.Release(currentMessageContext);
            }
        }

        bool ExecuteHandler(object message, Type messageType, MethodInfo handlerMethod, CurrentMessageContext currentMessageContext)
        {
            var handler = Container.Resolve(handlerMethod.DeclaringType);
            Action executeHandler = () => handlerMethod.Invoke(handler, new[] {message});


            if (IsSubclassOfRawGeneric(typeof (Saga<>), handlerMethod.DeclaringType)) //this must be a saga, whether existing or a new one is a diffirent question
            {
                var sagaDataType = handlerMethod.DeclaringType.BaseType.GenericTypeArguments.First();
                var startSagaTypes = MessageToStartSagaMapping.ResolveRouteFor(messageType);
                var sagaData = startSagaTypes != null ? SagaManager.Start(sagaDataType, currentMessageContext) : SagaManager.Resume(sagaDataType, currentMessageContext);

                if (sagaData == null)
                {
                    Console.WriteLine("Warning: SagaNotFound {0}<{1}> for incoming message {2}", handlerMethod.DeclaringType, sagaDataType.Name, messageType.Name);
                    return false;
                }

                var sagaDataProperty = handler.GetType().GetProperty("Data");
                sagaDataProperty.SetValue(handler, sagaData.SagaState);

                try {
                    executeHandler();
                }
                finally
                {
                    if (((ISaga) handler).IsClosed)
                        SagaStorage.Close(sagaData.SagaId);
                    else
                        SagaStorage.Update(sagaData.SagaId, sagaData.SagaState);
                }
            }
            else
            {
                //normal saga-less handler
                executeHandler();
            }

            return true;
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

        /// <summary>
        ///     TODO: Don't have use fot this event now
        /// </summary>
        void TransportOnTransportMessageFinished(object sender, TransportMessageFinishedEventArgs transportMessageFinishedEventArgs)
        {
        }


        void ApplyHandlerMapping(IEnumerable<Type> events, IEnumerable<Type> commands, IEnumerable<Type> allTypes)
        {
            var eventsAndCommands = events.Union(commands).ToArray();

            foreach (var mapping in EffectiveConfiguration.NamespaceToEndpointMappings)
            {
                var types = eventsAndCommands.Where(type => type.Namespace != null && type.Namespace.Equals(mapping.Namespace, StringComparison.InvariantCultureIgnoreCase)).ToList();
                var eventsAndCommandsInANamespace = eventsAndCommands.Intersect(types).Distinct().ToList();
                var notMappedTypes = types.Except(eventsAndCommandsInANamespace);

                if (notMappedTypes.Any())
                    throw new Exception("Some messages are not mapped: " + notMappedTypes.Select(x => x.Name).Aggregate((a, b) => a + ", " + b));

                MessageRoutingTable.RouteTypes(eventsAndCommandsInANamespace, mapping.Address);
            }

            Func<Type, Type, bool> implementsType = (y, compareType) => y.IsGenericType && y.GetGenericTypeDefinition() == compareType;

            var handlers = allTypes.Where(EffectiveConfiguration.IsHandler).SelectMany(type => type.GetMethods()
                                                                                                   .Where(x => x.GetParameters().Select(parameter => parameter.ParameterType)
                                                                                                                .Where(type.GetInterfaces()
                                                                                                                           .Where(intface => implementsType(intface, typeof (IHandle<>)))
                                                                                                                           .SelectMany(intfs => intfs.GenericTypeArguments).Contains)
                                                                                                                .Any()));

            foreach (var methodInfo in handlers)
            {
                var firstParameter = methodInfo.GetParameters().FirstOrDefault();
                if (firstParameter == null) continue;

                HandlerRoutingTable.RouteTypes(new[] {firstParameter.ParameterType}, methodInfo);

                var methods = HandlerRoutingTable.ResolveRouteFor(firstParameter.ParameterType);
                foreach (var method in methods)
                    Container.Configure(method.DeclaringType, ComponentLifecycle.PerUnitOfWork);
            }


            var enumerable = allTypes.Where(x => x.GetInterfaces()
                                                  .Any(intface => implementsType(intface, typeof (IStartThisSagaWhenReceive<>)))).ToList();
            var messageToStartSagaMapping = enumerable
                .SelectMany(type => type.GetInterfaces()
                                        .Where(intface => implementsType(intface, typeof (IStartThisSagaWhenReceive<>)))
                                        .Where(intfs => intfs.IsGenericType && intfs.GetGenericArguments().Any())
                                        .Select(y => new {type, message = y.GenericTypeArguments.First()})).ToList();

            messageToStartSagaMapping.ForEach(x => MessageToStartSagaMapping.RouteType(x.message, x.type));
        }
    }
}