namespace Idecom.Bus.Implementations.UnicastBus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Addressing;
    using Interfaces;
    using Interfaces.Addons.PubSub;
    using Internal;
    using Transport;
    using Utility;

    internal class Bus : IBusInstance
    {
        private bool _isStarted;
        public IContainer Container { get; set; }
        public IRoutingTable<MethodInfo> HandlerRoutingTable { get; set; }
        public IRoutingTable<Address> MessageRoutingTable { get; set; }
        public IRoutingTable<Type> SagaRoutingTable { get; set; }
        public IMessageSerializer Serializer { get; set; }
        public IInstanceCreator InstanceCreator { get; set; }
        public ISubscriptionDistributor SubscriptionDistributor { get; set; }
        public ITransport Transport { get; set; }
        public Address LocalAddress { get; set; }
        public IEffectiveConfiguration EffectiveConfiguration { get; set; }
        public ISagaStorage SagaStorage { get; set; }


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
                var commands = allTypes.Where(EffectiveConfiguration.IsCommand);
                ApplyHandlerMapping(events, commands, allTypes);


                Transport.TransportMessageReceived += TransportMessageReceived;
                Transport.TransportMessageFinished += TransportOnTransportMessageFinished;

                foreach (var beforeBusStarted in Container.ResolveAll<IBeforeBusStarted>())
                {
                    beforeBusStarted.BeforeBusStarted();
                    Container.Release(beforeBusStarted);
                }

                SubscriptionDistributor.SubscribeTo(events);

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

            ExecuteOnlyWhenStarted(() => Transport.Send(new TransportMessage(message, LocalAddress, MessageRoutingTable.ResolveRouteFor(message.GetType()), MessageIntent.Send), CurrentMessageContextInternal()));
        }

        public void SendLocal(object message)
        {
            ExecuteOnlyWhenStarted(() => Transport.Send(new TransportMessage(message, LocalAddress, MessageRoutingTable.ResolveRouteFor(message.GetType()), MessageIntent.SendLocal), CurrentMessageContextInternal()));
        }

        public void Reply(object message)
        {
            if (LocalAddress.Equals(CurrentMessageContext.TransportMessage.SourceAddress))
                throw new Exception("Received a message with reply address as a local queue. This can cause an infinite loop and been stopped. Queue: " + CurrentMessageContext.TransportMessage.SourceAddress);

            ExecuteOnlyWhenStarted(() => Transport.Send(new TransportMessage(message, LocalAddress, MessageRoutingTable.ResolveRouteFor(message.GetType()), MessageIntent.Reply), CurrentMessageContextInternal()));
        }

        public void Raise<T>(Action<T> action) where T : class
        {
            var message = InstanceCreator.CreateInstanceOf<T>();
            action(message);
            SubscriptionDistributor.NotifySubscribersOf<T>(message, CurrentMessageContextInternal());
        }

        private CurrentMessageContext CurrentMessageContextInternal()
        {
            var currentMessageContext = Container.Resolve<CurrentMessageContext>();
            Container.Release(currentMessageContext);
            return currentMessageContext;
        }

        private void ExecuteOnlyWhenStarted(Action todo)
        {
            if (_isStarted || CurrentMessageContext != null)
                todo();
            else
                throw new Exception("Can not send or receive messages while the bus is stopped.");
        }

        private void TransportMessageReceived(object sender, TransportMessageReceivedEventArgs e)
        {
            CurrentMessageContext currentMessageContext = null;
            try
            {
                currentMessageContext = Container.Resolve<CurrentMessageContext>();
                currentMessageContext.TransportMessage = e.TransportMessage;
                currentMessageContext.Attempt = e.Attempt;
                currentMessageContext.MaxAttempts = e.MaxRetries + 1;

                var handlerMethod = HandlerRoutingTable.ResolveRouteFor(e.TransportMessage.MessageType);
                var handler = Container.Resolve(handlerMethod.DeclaringType);

                var sagaClass = SagaRoutingTable.ResolveRouteFor(e.TransportMessage.MessageType);
                if (sagaClass != null)
                {
                    var sagaStateClass = sagaClass.BaseType.GenericTypeArguments.FirstOrDefault();
                    if (currentMessageContext.TransportMessage.Headers.ContainsKey(SystemHeaders.SAGA_ID))
                    {
                        var existingSagaId = currentMessageContext.TransportMessage.Headers[SystemHeaders.SAGA_ID];
                        currentMessageContext.ResumeSaga(existingSagaId);
                    }
                    else
                    {
                        currentMessageContext.StartSaga();
                        var sagaStateInstance = InstanceCreator.CreateInstanceOf(sagaStateClass);

                        var state = handler.GetType().GetProperty("State");
                        state.SetValue(handler, sagaStateInstance);

                    }
                }


                handlerMethod.Invoke(handler, new[] {e.TransportMessage.Message});


            }
            catch (Exception) {
            }
            finally { Container.Release(currentMessageContext); }
        }

        private void TransportOnTransportMessageFinished(object sender, TransportMessageFinishedEventArgs transportMessageFinishedEventArgs)
        {
            var currentMessageContext = CurrentMessageContextInternal();

            foreach (var action in currentMessageContext.DelayedSends)
            {
                //Track the saga through handlers
                if (currentMessageContext.Headers.ContainsKey(SystemHeaders.SAGA_ID)) {
                    action.TransportMessage.Headers[SystemHeaders.SAGA_ID] = currentMessageContext.Headers[SystemHeaders.SAGA_ID];
                }
                Transport.Send(action.TransportMessage);
            }
        }


        private void ApplyHandlerMapping(IEnumerable<Type> events, IEnumerable<Type> commands, IEnumerable<Type> allTypes)
        {
            var eventsAndCommands = events.Union(commands).ToList();

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

            var handlers = allTypes.SelectMany(type => type.GetMethods()
                                                           .Where(x => x.GetParameters().Select(parameter => parameter.ParameterType)
                                                                        .Where(type.GetInterfaces()
                                                                                   .Where(intface => implementsType(intface, typeof (IHandle<>)))
                                                                                   .SelectMany(intfs => intfs.GenericTypeArguments).Contains)
                                                                        .Any()));

            foreach (MethodInfo methodInfo in handlers)
            {
                var firstParameter = methodInfo.GetParameters().FirstOrDefault();
                if (firstParameter == null) continue;

                HandlerRoutingTable.RouteTypes(new[] {firstParameter.ParameterType}, methodInfo);
                var method = HandlerRoutingTable.ResolveRouteFor(firstParameter.ParameterType);
                Container.Configure(method.DeclaringType, ComponentLifecycle.PerUnitOfWork);
            }


            var messageToStoryMapping = allTypes.Where(x => x.GetInterfaces()
                .Any(intface => implementsType(intface, typeof(IStartThisSagaWhenReceive<>))))
            .SelectMany(type => type.GetInterfaces()
                                      .Where(intface => implementsType(intface, typeof(IStartThisSagaWhenReceive<>)))
                                      .Where(intfs => intfs.IsGenericType && intfs.GetGenericArguments().Any())
                                      .Select(y => new { type, message = y.GenericTypeArguments.First() })).ToList();

            messageToStoryMapping.ForEach(x => SagaRoutingTable.RouteType(x.message, x.type));

        }
    }
}