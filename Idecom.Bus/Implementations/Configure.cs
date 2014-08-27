namespace Idecom.Bus.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Addons.PubSub;
    using Addressing;
    using Behaviors;
    using Interfaces;
    using Interfaces.Behaviors;
    using Internal;
    using UnicastBus;

    public class Configure
    {
        readonly List<NamespaceToEndpointMapping> _namespaceToEndpoints;

        IContainer _container;

        protected Configure()
        {
            _namespaceToEndpoints = new List<NamespaceToEndpointMapping>();
        }

        public IContainer Container
        {
            get { return _container; }
            internal set
            {
                value.ConfigureInstance(new RoutingTable<Address>());
                value.ConfigureInstance(new PluralRoutingTable<MethodInfo>());
                value.ConfigureInstance(new RoutingTable<Type>());

                value.Configure<EffectiveConfiguration>(ComponentLifecycle.Singleton);

                value.ConfigureProperty<EffectiveConfiguration>(x => x.IsEvent, DefaultConfiguration.DefaultEventNamingConvention);
                value.ConfigureProperty<EffectiveConfiguration>(x => x.IsCommand, DefaultConfiguration.DefaultCommandNamingConvention);
                value.ConfigureProperty<EffectiveConfiguration>(x => x.IsHandler, DefaultConfiguration.DefaultHandlerConvention);
                value.ConfigureProperty<EffectiveConfiguration>(x => x.NamespaceToEndpointMappings, _namespaceToEndpoints);

                value.Configure<InstanceCreator>(ComponentLifecycle.Singleton);
                value.Configure<Bus>(ComponentLifecycle.Singleton);
                value.Configure<SubscriptionDistributor>(ComponentLifecycle.Singleton);
                value.Configure<SagaManager>(ComponentLifecycle.Singleton);
                
                value.Configure<ChainExecutor>(ComponentLifecycle.PerUnitOfWork);
                value.Configure<BehaviorChains>(ComponentLifecycle.Singleton);
                
                value.Configure<MessageToEndpointRoutingTable>(ComponentLifecycle.Singleton);
                value.Configure<MessageToHandlerRoutingTable>(ComponentLifecycle.Singleton);
                
                value.Configure<HandlerContext>(ComponentLifecycle.PerUnitOfWork);
                value.Configure<ChainExecutionContext>(ComponentLifecycle.PerUnitOfWork);


                _container = value;
            }
        }


        public static ConfigureContainer With()
        {
            return new ConfigureContainer(new Configure());
        }

        public IBusInstance CreateBus(string queueName = null)
        {
            Container.ConfigureInstance(new Address(queueName));

            var bus = Container.Resolve<IBusInstance>();

            Container.ParentContainer.ConfigureInstance(bus);
            Container.Release(bus);
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
    }
}