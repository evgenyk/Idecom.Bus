namespace Idecom.Bus.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Addons.PubSub;
    using Addressing;
    using Behaviors;
    using Interfaces;
    using Interfaces.Logging;
    using Internal;
    using UnicastBus;

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
            }
        }

        public List<NamespaceToEndpointMapping> NamespaceToEndpoints
        {
            get { return _namespaceToEndpoints; }
        }

        public static ConfigureContainer With()
        {
            return new ConfigureContainer(new Configure());
        }

        public IBusInstance CreateBus(string queueName = null)
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