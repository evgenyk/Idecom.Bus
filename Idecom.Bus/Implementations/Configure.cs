﻿namespace Idecom.Bus.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Addons.PubSub;
    using Addressing;
    using Behaviors;
    using Interfaces;
    using Interfaces.Logging;
    using Internal;
    using UnicastBus;
    using Utility;

    public class DebugView: IDebugView
    {
        public IList<Type> EventsDiscovered { get; set; }
        public IList<Type> CommandsDiscovered { get; set; }
        public IList<Type> EventsWithHandlers { get; set; }
        public IList<IBeforeBusStarted> BeforeStartedDiscovered { get; set; }
        public List<ClassToMessageInfo> Handlers { get; set; }
        public List<ClassToMessageInfo> Sagas { get; set; }

        public DebugView()
        {
            Handlers = new List<ClassToMessageInfo>();
            Sagas = new List<ClassToMessageInfo>();
        }

        public void RecordEventsWithHandlers(IEnumerable<Type> eventsWithHandlers)
        {
            EventsWithHandlers = new List<Type>(eventsWithHandlers);
        }

        public void RecordEventsDiscovered(IEnumerable<Type> events)
        {
            EventsDiscovered = new List<Type>(events);
        }

        public void RecordCommandsDiscovered(IEnumerable<Type> commands)
        {
            CommandsDiscovered = new List<Type>(commands);
        }

        public void RecordBeforeStarted(List<IBeforeBusStarted> beforeBusStarteds)
        {
            BeforeStartedDiscovered = new List<IBeforeBusStarted>(beforeBusStarteds);
        }

        public void RecordHandler(Type handlerType, Type messageType)
        {
            Handlers.Add(new ClassToMessageInfo(handlerType, messageType));
        }

        public void SagaStarts(Type sagaType, Type messageType)
        {
            Sagas.Add(new ClassToMessageInfo(sagaType, messageType));
        }
    }

    public class ClassToMessageInfo
    {
        readonly Type _handlerType;
        readonly Type _messageType;
        public override string ToString()
        {
            return string.Format("Handler: {0}, message: {1}", _handlerType.Name, _messageType.Name);
        }

        public ClassToMessageInfo(Type handlerType, Type messageType)
        {
            _handlerType = handlerType;
            _messageType = messageType;
        }
    }

    public class Configure
    {
        readonly List<NamespaceToEndpointMapping> _namespaceToEndpoints;
        IContainer _container;

        Configure()
        {
            _namespaceToEndpoints = new List<NamespaceToEndpointMapping>();
        }

        public IContainer Container
        {
            get { return _container; }
            internal set
            {
                _container = value;

                _container.Configure<EffectiveConfiguration>(ComponentLifecycle.Singleton);

                _container.ConfigureProperty<EffectiveConfiguration>(x => x.IsEvent, DefaultConfiguration.DefaultEventNamingConvention);
                _container.ConfigureProperty<EffectiveConfiguration>(x => x.IsCommand, DefaultConfiguration.DefaultCommandNamingConvention);
                _container.ConfigureProperty<EffectiveConfiguration>(x => x.IsHandler, DefaultConfiguration.DefaultHandlerConvention);
                _container.ConfigureProperty<EffectiveConfiguration>(x => x.NamespaceToEndpointMappings, new List<NamespaceToEndpointMapping>());

                _container.ConfigureInstance(new RoutingTable<Address>());
                _container.ConfigureInstance(new PluralRoutingTable<MethodInfo>());
                _container.ConfigureInstance(new RoutingTable<Type>());

                _container.Configure<InstanceCreator>(ComponentLifecycle.Singleton);

                _container.Configure<SagaManager>(ComponentLifecycle.Singleton);

                _container.Configure<ChainExecutor>(ComponentLifecycle.PerUnitOfWork);
                _container.Configure<BehaviorChains>(ComponentLifecycle.Singleton);

                _container.Configure<MessageToEndpointRoutingTable>(ComponentLifecycle.Singleton);
                _container.Configure<MessageToHandlerRoutingTable>(ComponentLifecycle.Singleton);
                _container.Configure<MessageToStartSagaMapping>(ComponentLifecycle.Singleton);

                _container.Configure<IncommingMessageContext>(ComponentLifecycle.PerUnitOfWork);
                _container.Configure<OutgoingMessageContext>(ComponentLifecycle.PerUnitOfWork);
                
                _container.Configure<DebugView>(ComponentLifecycle.Singleton);
            }
        }

        public List<NamespaceToEndpointMapping> NamespaceToEndpoints => _namespaceToEndpoints;

        public static ConfigureContainer WithContainer()
        {
            return new ConfigureContainer(new Configure());
        }

        public IBusInstance CreateInstance()
        {
            var nameFromAssemblyInfo = AssemblyScanner.GetScannableAssemblies().Where(x =>
                                                                                      {
                                                                                          var assemblyMetadataAttribute = x.GetCustomAttributes<AssemblyMetadataAttribute>();
                                                                                          return assemblyMetadataAttribute.Any() && assemblyMetadataAttribute.Select(y=>y.Key).Contains("idecom.endpoint.id");
                                                                                      }).ToList();
            if (nameFromAssemblyInfo.Count > 1)
                throw new Exception($"Found more than one AssemblyInfoAttributes with key idecom.endpoint.id in scanned assemblies: {nameFromAssemblyInfo.Select(x => x.FullName).Aggregate((a, b) => $"{a}, {b}")}");

            var name = nameFromAssemblyInfo.First().GetCustomAttributes<AssemblyMetadataAttribute>().First(attribute => attribute.Key.Equals("idecom.endpoint.id")).Value;

            return CreateInstance(name);
        }

        public IBusInstance CreateInstance(string queueName)
        {
            Container.ConfigureInstance(new Address(queueName));
            Container.Configure<RoutingAwareSubscriptionDistributor>(ComponentLifecycle.Singleton);
            Container.ConfigureProperty<EffectiveConfiguration>(x => x.NamespaceToEndpointMappings, _namespaceToEndpoints);

            Container.Configure<Bus>(ComponentLifecycle.Singleton);

            var bus = Container.Resolve<IBusInstance>();

            return bus;
        }

        public Configure DefineEventsAs(Func<Type, bool> eventsDefinition)
        {
            Container.ConfigureProperty<EffectiveConfiguration>(x => x.IsEvent, eventsDefinition);
            return this;
        }

        public Configure DefineCommandsAs(Func<Type, bool> commandsDefinition)
        {
            Container.ConfigureProperty<EffectiveConfiguration>(x => x.IsCommand, commandsDefinition);
            return this;
        }

        public Configure DefineHandlersAs(Func<Type, bool> handlerDefinition)
        {
            Container.ConfigureProperty<EffectiveConfiguration>(x => x.IsHandler, handlerDefinition);
            return this;
        }


        public Configure RouteMessagesFromNamespaceTo<T>(string address)
        {
            _namespaceToEndpoints.Add(new NamespaceToEndpointMapping(typeof (T).Namespace, new Address(address)));
            return this;
        }

        public Configure ExposeConfiguration(Action<Configure> exposeConfigurationAction)
        {
            exposeConfigurationAction(this);
            return this;
        }

        public void SetLogger(Func<string, ILog> func)
        {
            var loggerFactory = new LogFactory(func);
            _container.ConfigureInstance(loggerFactory);
        }
    }
}