namespace Idecom.Bus.Implementations.UnicastBus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Addressing;
    using Annotations;
    using Behaviors;
    using Interfaces;
    using Interfaces.Addons.PubSub;
    using Interfaces.Addons.Sagas;
    using Interfaces.Behaviors;
    using Interfaces.Logging;
    using Internal;
    using Utility;

    [LoggerName("Bus")]
    public class Bus : IBusInstance
    {
        bool _isStarted;

        [UsedImplicitly]
        public ILog Log { get; set; }

        [UsedImplicitly]
        public IContainer Container { get; set; }

        [UsedImplicitly]
        public IMessageToHandlerRoutingTable MessageToHandlerRoutingTable { get; set; }

        [UsedImplicitly]
        public IMessageToEndpointRoutingTable MessageRoutingTable { get; set; }

        [UsedImplicitly]
        public IMessageToStartSagaMapping MessageToStartSagaMapping { get; set; }

        [UsedImplicitly]
        public IMessageSerializer Serializer { get; set; }

        [UsedImplicitly]
        public IInstanceCreator InstanceCreator { get; set; }

        [UsedImplicitly]
        public ISubscriptionDistributor SubscriptionDistributor { get; set; }

        [UsedImplicitly]
        public ITransport Transport { get; set; }

        [UsedImplicitly]
        public Address LocalAddress { get; set; }

        [UsedImplicitly]
        public IEffectiveConfiguration EffectiveConfiguration { get; set; }

        [UsedImplicitly]
        public IBehaviorChains Chains { get; set; }


        public IMessageContext IncomingMessageContext
        {
            get { return AmbientChainContext.Current.IncomingMessageContext; }
        }

        public bool IsStarted
        {
            get { return _isStarted; }
        }

        public IBusInstance Start()
        {
            lock (this)
            {
                Log.Debug("Something happened...");

                if (_isStarted)
                    throw new ArgumentException("Can't start bus which already started.");
                if (Transport == null)
                    throw new ArgumentException("Can not create bus. Transport hasn't been provided or misconfigured.");
                if (Container == null)
                    throw new ArgumentException("Can not create bus. Container hasn't been provided or misconfigured.");
                if (Serializer == null)
                    throw new ArgumentException("Can not create bus. Message serializer hasn't been provided or misconfigured.");


                Container.Configure<Bus>(ComponentLifecycle.Singleton);

                var allTypes = AssemblyScanner.GetTypes().ToList();
                var events = allTypes.Where(EffectiveConfiguration.IsEvent).ToList();
                var commands = allTypes.Where(EffectiveConfiguration.IsCommand).ToList();
                ApplyHandlerMapping(events, commands, allTypes);
                var eventsWithHandlers = events.Where(e => MessageToHandlerRoutingTable.ResolveRouteFor(e).Any()).ToList();

                var behaviors = allTypes.Where(x => typeof (IBehavior).IsAssignableFrom(x) && !x.IsInterface).ToList();
                behaviors.ForEach(x => Container.Configure(x, ComponentLifecycle.PerUnitOfWork));

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
            using (var executionContext = AmbientChainContext.Current.Push(context =>
                                                                           {
                                                                               context.OutgoingMessage = message;
                                                                               context.OutgoingMessageType = message.GetType();
                                                                           })) {
                                                                               new ChainExecutor(Container).RunWithIt(Chains.GetChainFor(ChainIntent.Send), executionContext);
                                                                           }
        }

        public void SendLocal(object message)
        {
            using (var executionContext = AmbientChainContext.Current.Push(context => { context.OutgoingMessage = message; })) {
                new ChainExecutor(Container).RunWithIt(Chains.GetChainFor(ChainIntent.SendLocal), executionContext);
            }
        }

        public void Reply(object message)
        {
            if (LocalAddress.Equals(AmbientChainContext.Current.IncomingMessageContext.SourceAddress))
                throw new Exception(string.Format("Received a message with reply address as a local queue. This can cause an infinite loop and been stopped. Queue: {0}",
                    AmbientChainContext.Current.IncomingMessageContext.SourceAddress));

            var executor = new ChainExecutor(Container);

            using (var executionContext = AmbientChainContext.Current.Push(context =>
                                                                           {
                                                                               context.OutgoingMessage = message;
                                                                               context.OutgoingMessageType = message.GetType();
                                                                           })) {
                                                                               executor.RunWithIt(Chains.GetChainFor(ChainIntent.Reply), executionContext);
                                                                           }
        }

        public void Publish<T>(Action<T> action = null) where T : class
        {
            var message = InstanceCreator.CreateInstanceOf<T>();
            if (action != null) action(message);

            var executor = new ChainExecutor(Container);

            using (var context = AmbientChainContext.Current.Push(childContext =>
                                                                  {
                                                                      childContext.OutgoingMessage = message;
                                                                      childContext.OutgoingMessageType = typeof (T);
                                                                  }))
            {
                var behaviorChain = Chains.GetChainFor(ChainIntent.Publish);

                executor.RunWithIt(behaviorChain, context);
            }
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

                MessageToHandlerRoutingTable.RouteTypes(new[] {firstParameter.ParameterType}, methodInfo);

                var methods = MessageToHandlerRoutingTable.ResolveRouteFor(firstParameter.ParameterType);
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