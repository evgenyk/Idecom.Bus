using System.Collections.Generic;
using System.Reflection;
using Idecom.Bus.Addressing;
using Idecom.Bus.Implementations.Addons.PubSub;
using Idecom.Bus.Implementations.Internal;
using Idecom.Bus.Interfaces;

namespace Idecom.Bus.Implementations
{
    public class Configure
    {
        private readonly List<NamespaceToEndpointMapping> _namespaceToEndpoints;

        private IContainer _container;

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
                value.ConfigureInstance(new RoutingTable<MethodInfo>());

                value.Configure<EffectiveConfiguration>(ComponentLifecycle.Singleton);

                value.ConfigureProperty<EffectiveConfiguration>(x => x.IsEvent, DefaultConfiguration.DefaultEventNamingConvention);
                value.ConfigureProperty<EffectiveConfiguration>(x => x.IsCommand, DefaultConfiguration.DefaultCommandNamingConvention);
                value.ConfigureProperty<EffectiveConfiguration>(x => x.MessageMappings, _namespaceToEndpoints);

                value.Configure<InstanceCreator>(ComponentLifecycle.Singleton);
                value.Configure<UnicastBus.Bus>(ComponentLifecycle.Singleton);
                value.Configure<SubscriptionDistributor>(ComponentLifecycle.Singleton);
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

        public Configure RouteMessagesFromNamespaceTo<T>(string address)
        {
            _namespaceToEndpoints.Add(new NamespaceToEndpointMapping(typeof (T).Namespace, new Address(address)));
            return this;
        }
    }
}