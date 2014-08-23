namespace Idecom.Bus.Implementations.UnicastBus
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Addressing;
    using Behaviors;
    using Interfaces;
    using Interfaces.Addons.PubSub;
    using Interfaces.Addons.Sagas;
    using Interfaces.Behaviors;
    using Internal;
    using Transport;
    using Utility;

    class Bus : IBusInstance
    {
        bool _isStarted;
        public IContainer Container { get; set; }

        public IMessageToHandlerRoutingTable HandlerToHandlerRoutingTable { get; set; }
        public IMessageToEndpointRoutingTable MessageRoutingTable { get; set; }
        public IRoutingTable<Type> MessageToStartSagaMapping { get; set; }

        public IMessageSerializer Serializer { get; set; }
        public IInstanceCreator InstanceCreator { get; set; }
        public ISubscriptionDistributor SubscriptionDistributor { get; set; }
        public ITransport Transport { get; set; }
        public Address LocalAddress { get; set; }
        public IEffectiveConfiguration EffectiveConfiguration { get; set; }
        public ISagaStorage SagaStorage { get; set; }
        public ISagaManager SagaManager { get; set; }
        public IBehaviorChains Chains { get; set; }


        public IMessageContext CurrentMessageContext
        {
            get { return CurrentMessageContextInternal(); }
        }

        public bool IsStarted { get { return _isStarted; } }

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

                Container.Configure<MessageContext>(ComponentLifecycle.PerUnitOfWork);
                Container.Configure<OutgoingMessageContext>(ComponentLifecycle.PerUnitOfWork);


                var allTypes = AssemblyScanner.GetTypes().ToList();
                var events = allTypes.Where(EffectiveConfiguration.IsEvent).ToList();
                var commands = allTypes.Where(EffectiveConfiguration.IsCommand).ToList();
                ApplyHandlerMapping(events, commands, allTypes);
                var eventsWithHandlers = events.Where(e => HandlerToHandlerRoutingTable.ResolveRouteFor(e).Any()).ToList();

                var behaviors = allTypes.Where(x => typeof(IBehavior).IsAssignableFrom(x) && !x.IsInterface).ToList();
                behaviors.ForEach(x => Container.Configure(x, ComponentLifecycle.PerUnitOfWork));

                //                Transport.TransportMessageReceived += TransportMessageReceived;

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
            new ChainExecutor(Container).RunWithIt(Chains.GetChainFor(ChainIntent.Send), new ChainExecutionContext { OutgoingMessage = message });
        }

        public void SendLocal(object message)
        {
            new ChainExecutor(Container).RunWithIt(Chains.GetChainFor(ChainIntent.SendLocal), new ChainExecutionContext { OutgoingMessage = message });
        }

        public void Reply(object message)
        {
            if (LocalAddress.Equals(CurrentMessageContext.IncomingTransportMessage.SourceAddress))
                throw new Exception(string.Format("Received a message with reply address as a local queue. This can cause an infinite loop and been stopped. Queue: {0}",
                    CurrentMessageContext.IncomingTransportMessage.SourceAddress));

            Transport.Send(new TransportMessage(message, LocalAddress, MessageRoutingTable.ResolveRouteFor(message.GetType()), MessageIntent.Reply, message.GetType()));
        }

        public void Raise<T>(Action<T> action) where T : class
        {

            var message = InstanceCreator.CreateInstanceOf<T>();
            if (action != null) action(message);

            var executor = new ChainExecutor(Container);
            executor.RunWithIt(Chains.GetChainFor(ChainIntent.Publish), new ChainExecutionContext { OutgoingMessage = message, MessageType = typeof(T) });
        }

        [DebuggerStepThrough]
        MessageContext CurrentMessageContextInternal()
        {
            var currentMessageContext = Container.Resolve<MessageContext>();
            Container.Release(currentMessageContext);
            return currentMessageContext;
        }

        //        void TransportMessageReceived(object sender, TransportMessageReceivedEventArgs e)
        //        {
        //            MessageContext messageContext = null;
        //            try
        //            {
        //                messageContext = Container.Resolve<MessageContext>();
        //                if (messageContext == null)
        //                    throw new Exception("Could not resolve current message context");
        //
        //                messageContext.IncomingTransportMessage = e.TransportMessage;
        //                messageContext.Attempt = e.Attempt;
        //                messageContext.MaxAttempts = e.MaxRetries + 1;
        //
        //                var message = e.TransportMessage.Message;
        //                var type = e.TransportMessage.MessageType ?? message.GetType();
        //                var handlerMethods = HandlerToHandlerRoutingTable.ResolveRouteFor(type);
        //
        //                var executedHandlers = handlerMethods.Select(handler => ExecuteHandler(message, type, handler, messageContext)).ToList();
        //
        //                if (executedHandlers.All(x => !x))
        //                    Console.WriteLine("Warning: {1} received a message of type {0}, but could not find a handler for it", e.TransportMessage.MessageType, LocalAddress);
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine("Error while receiving a message: " + ex);
        //                throw;
        //            }
        //            finally {
        //                Container.Release(messageContext);
        //            }
        //        }

        bool ExecuteHandler(object message, Type messageType, MethodInfo handlerMethod, MessageContext messageContext)
        {
            var handler = Container.Resolve(handlerMethod.DeclaringType);
            Action executeHandler = () => handlerMethod.Invoke(handler, new[] { message });


            if (IsSubclassOfRawGeneric(typeof(Saga<>), handlerMethod.DeclaringType)) //this must be a saga, whether existing or a new one is a diffirent question
            {
                var sagaDataType = handlerMethod.DeclaringType.BaseType.GenericTypeArguments.First();
                var startSagaTypes = MessageToStartSagaMapping.ResolveRouteFor(messageType);
                ISagaStateInstance sagaData;
                if (startSagaTypes != null) sagaData = SagaManager.Start(sagaDataType, messageContext);
                else sagaData = SagaManager.Resume(sagaDataType, messageContext);

                if (sagaData == null)
                {
                    Console.WriteLine("Warning: SagaNotFound {0}<{1}> for incoming message {2}", handlerMethod.DeclaringType, sagaDataType.Name, messageType.Name);
                    return false;
                }

                var sagaDataProperty = handler.GetType().GetProperty("Data");
                sagaDataProperty.SetValue(handler, sagaData.SagaState);

                try
                {
                    executeHandler();
                }
                finally
                {
                    if (((ISaga)handler).IsClosed)
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
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur) { return true; }
                toCheck = toCheck.BaseType;
            }
            return false;
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
                                                                                                                           .Where(intface => implementsType(intface, typeof(IHandle<>)))
                                                                                                                           .SelectMany(intfs => intfs.GenericTypeArguments).Contains)
                                                                                                                .Any()));

            foreach (var methodInfo in handlers)
            {
                var firstParameter = methodInfo.GetParameters().FirstOrDefault();
                if (firstParameter == null) continue;

                HandlerToHandlerRoutingTable.RouteTypes(new[] { firstParameter.ParameterType }, methodInfo);

                var methods = HandlerToHandlerRoutingTable.ResolveRouteFor(firstParameter.ParameterType);
                foreach (var method in methods)
                    Container.Configure(method.DeclaringType, ComponentLifecycle.PerUnitOfWork);
            }


            var enumerable = allTypes.Where(x => x.GetInterfaces()
                                                  .Any(intface => implementsType(intface, typeof(IStartThisSagaWhenReceive<>)))).ToList();
            var messageToStartSagaMapping = enumerable
                .SelectMany(type => type.GetInterfaces()
                                        .Where(intface => implementsType(intface, typeof(IStartThisSagaWhenReceive<>)))
                                        .Where(intfs => intfs.IsGenericType && intfs.GetGenericArguments().Any())
                                        .Select(y => new { type, message = y.GenericTypeArguments.First() })).ToList();

            messageToStartSagaMapping.ForEach(x => MessageToStartSagaMapping.RouteType(x.message, x.type));
        }
    }

}