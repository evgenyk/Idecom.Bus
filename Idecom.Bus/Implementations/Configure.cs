namespace Idecom.Bus.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Addons.PubSub;
    using Addressing;
    using Behaviors;
    using Interfaces;
    using Internal;
    using UnicastBus;

    public class Configure
    {
        //readonly List<NamespaceToEndpointMapping> _namespaceToEndpoints;

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