using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Idecom.Bus.Addressing;
using Idecom.Bus.Implementations.Internal;
using Idecom.Bus.Interfaces;
using Idecom.Bus.Interfaces.Addons.PubSub;
using Idecom.Bus.Interfaces.Addons.Stories;
using Idecom.Bus.Transport;
using Idecom.Bus.Utility;

namespace Idecom.Bus.Implementations.UnicastBus
{
    internal class Bus : IBusInstance
    {
        private bool _isStarted;
        public IContainer Container { get; set; }
        public IRoutingTable<MethodInfo> HandlerRoutingTable { get; set; }
        public IRoutingTable<Address> MessageRoutingTable { get; set; }
        public IRoutingTable<Type> MessageToStartSagaMapping { get; set; }
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


                List<Type> allTypes = AssemblyScanner.GetTypes().ToList();
                List<Type> events = allTypes.Where(EffectiveConfiguration.IsEvent).ToList();
                List<Type> commands = allTypes.Where(EffectiveConfiguration.IsCommand).ToList();
                ApplyHandlerMapping(events, commands, allTypes);

                List<Type> eventsWithHandlers = events.Where(e => HandlerRoutingTable.ResolveRouteFor(e) != null).ToList();


                Transport.TransportMessageReceived += TransportMessageReceived;
                Transport.TransportMessageFinished += TransportOnTransportMessageFinished;

                foreach (IBeforeBusStarted beforeBusStarted in Container.ResolveAll<IBeforeBusStarted>())
                {
                    beforeBusStarted.BeforeBusStarted();
                    Container.Release(beforeBusStarted);
                }

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
                foreach (IBeforeBusStopped beforeBusStopped in Container.ResolveAll<IBeforeBusStopped>())
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
            ExecuteOnlyWhenStarted(
                () => Transport.Send(new TransportMessage(message, LocalAddress, MessageRoutingTable.ResolveRouteFor(message.GetType()), MessageIntent.SendLocal), CurrentMessageContextInternal()));
        }

        public void Reply(object message)
        {
            if (LocalAddress.Equals(CurrentMessageContext.TransportMessage.SourceAddress))
                throw new Exception("Received a message with reply address as a local queue. This can cause an infinite loop and been stopped. Queue: " +
                                    CurrentMessageContext.TransportMessage.SourceAddress);

            ExecuteOnlyWhenStarted(
                () => Transport.Send(new TransportMessage(message, LocalAddress, MessageRoutingTable.ResolveRouteFor(message.GetType()), MessageIntent.Reply), CurrentMessageContextInternal()));
        }

        public void Raise<T>(Action<T> action) where T : class
        {
            var message = InstanceCreator.CreateInstanceOf<T>();
            action(message);
            SubscriptionDistributor.NotifySubscribersOf<T>(message, CurrentMessageContextInternal());
        }

        [DebuggerStepThrough]
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

                MethodInfo handlerMethod = HandlerRoutingTable.ResolveRouteFor(e.TransportMessage.MessageType);
                if (handlerMethod == null)
                {
                    throw new Exception("Could not resolve handler for " + e.TransportMessage.MessageType);
                }
                object handler = Container.Resolve(handlerMethod.DeclaringType);
                Action executeHandler = () => handlerMethod.Invoke(handler, new[] {e.TransportMessage.Message});

                Type startSagaType = MessageToStartSagaMapping.ResolveRouteFor(e.TransportMessage.MessageType);
                bool inSaga = IsSubclassOfRawGeneric(typeof (Saga<>), handlerMethod.DeclaringType) &&
                              (startSagaType != null || currentMessageContext.TransportMessage.Headers.ContainsKey(SystemHeaders.SAGA_ID));


                string sagaId = null;

                if (currentMessageContext.TransportMessage.Headers.ContainsKey(SystemHeaders.SAGA_ID))
                    sagaId = currentMessageContext.TransportMessage.Headers[SystemHeaders.SAGA_ID];

                bool isSagaSingleton = inSaga && handlerMethod.DeclaringType.GetCustomAttribute<SingleInstanceSagaAttribute>() != null;

                if (sagaId == null && isSagaSingleton)
                    sagaId = startSagaType.FullName.ToLowerInvariant();

                if (inSaga)
                {
                    if (sagaId != null)
                        currentMessageContext.ResumeSaga(sagaId);


                    object sagaData = null;

                    if (sagaId != null)
                        sagaData = SagaStorage.Get(sagaId);

                    if (sagaData == null)
                    {
                        sagaId = currentMessageContext.StartSaga(sagaId);
                        Type sagaStateClass = startSagaType.BaseType.GenericTypeArguments.FirstOrDefault();
                        sagaData = InstanceCreator.CreateInstanceOf(sagaStateClass);
                    }

                    PropertyInfo sagaDataProperty = handler.GetType().GetProperty("Data");
                    sagaDataProperty.SetValue(handler, sagaData);

                    try
                    {
                        executeHandler();
                    }
                    finally
                    {
                        if (((ISaga) handler).IsClosed)
                            SagaStorage.Close(sagaId);
                        else
                            SagaStorage.Update(sagaId, sagaData);
                    }
                }
                else
                {
                    executeHandler();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
            finally
            {
                Container.Release(currentMessageContext);
            }
        }

        private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof (object))
            {
                Type cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }

        private void TransportOnTransportMessageFinished(object sender, TransportMessageFinishedEventArgs transportMessageFinishedEventArgs)
        {
            CurrentMessageContext currentMessageContext = CurrentMessageContextInternal();

            //Track the saga through handlers
            foreach (DelayedSend action in currentMessageContext.DelayedSends)
            {
                if (currentMessageContext.Headers.ContainsKey(SystemHeaders.SAGA_ID))
                    // attaching to a new saga created in this message
                    action.TransportMessage.Headers[SystemHeaders.SAGA_ID] = currentMessageContext.Headers[SystemHeaders.SAGA_ID];
                else if (currentMessageContext.TransportMessage != null && currentMessageContext.TransportMessage.Headers.ContainsKey(SystemHeaders.SAGA_ID))
                    // attaching to saga in the incoming message
                    action.TransportMessage.Headers[SystemHeaders.SAGA_ID] = currentMessageContext.TransportMessage.Headers[SystemHeaders.SAGA_ID];
                Transport.Send(action.TransportMessage);
            }
        }


        private void ApplyHandlerMapping(IEnumerable<Type> events, IEnumerable<Type> commands, IEnumerable<Type> allTypes)
        {
            List<Type> eventsAndCommands = events.Union(commands).ToList();

            foreach (NamespaceToEndpointMapping mapping in EffectiveConfiguration.NamespaceToEndpointMappings)
            {
                List<Type> types = eventsAndCommands.Where(type => type.Namespace != null && type.Namespace.Equals(mapping.Namespace, StringComparison.InvariantCultureIgnoreCase)).ToList();
                List<Type> eventsAndCommandsInANamespace = eventsAndCommands.Intersect(types).Distinct().ToList();
                IEnumerable<Type> notMappedTypes = types.Except(eventsAndCommandsInANamespace);

                if (notMappedTypes.Any())
                    throw new Exception("Some messages are not mapped: " + notMappedTypes.Select(x => x.Name).Aggregate((a, b) => a + ", " + b));

                MessageRoutingTable.RouteTypes(eventsAndCommandsInANamespace, mapping.Address);
            }

            Func<Type, Type, bool> implementsType = (y, compareType) => y.IsGenericType && y.GetGenericTypeDefinition() == compareType;

            IEnumerable<MethodInfo> handlers = allTypes.SelectMany(type => type.GetMethods()
                .Where(x => x.GetParameters().Select(parameter => parameter.ParameterType)
                    .Where(type.GetInterfaces()
                        .Where(intface => implementsType(intface, typeof (IHandle<>)))
                        .SelectMany(intfs => intfs.GenericTypeArguments).Contains)
                    .Any()));

            foreach (MethodInfo methodInfo in handlers)
            {
                ParameterInfo firstParameter = methodInfo.GetParameters().FirstOrDefault();
                if (firstParameter == null) continue;

                HandlerRoutingTable.RouteTypes(new[] {firstParameter.ParameterType}, methodInfo);
                MethodInfo method = HandlerRoutingTable.ResolveRouteFor(firstParameter.ParameterType);
                Container.Configure(method.DeclaringType, ComponentLifecycle.PerUnitOfWork);
            }


            List<Type> enumerable = allTypes.Where(x => x.GetInterfaces()
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