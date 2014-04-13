using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Idecom.Bus.Addressing;
using Idecom.Bus.Interfaces;
using Idecom.Bus.Utility;

namespace Idecom.Bus.Implementations
{
    public class Configure
    {
        private readonly RoutingTable<MethodInfo> _handlerRoutingTable;
        private readonly RoutingTable<Address> _messageRoutingTable;
        private IContainer _container;
        private bool _handlersMapped;

        protected Configure()
        {
            _messageRoutingTable = new RoutingTable<Address>();
            _handlerRoutingTable = new RoutingTable<MethodInfo>();
            _handlersMapped = false;
        }

        public IContainer Container
        {
            get { return _container; }
            internal set
            {
                _container = value;
                _container.ConfigureInstance(_messageRoutingTable);
                _container.ConfigureInstance(_handlerRoutingTable);
            }
        }


        public static ConfigureContainer With()
        {
            var instance = new ConfigureContainer(new Configure());
            return instance;
        }

        public IBusInstance CreateBus(string queueName = null, int workersCount = 1, int retries = 1)
        {
            if (!_handlersMapped)
                ApplyDefaultHandlerMapping();

            Container.Configure<UnicastBus.Bus>(Lifecycle.Singleton);
            Container.ConfigureProperty<UnicastBus.Bus>(x => x.QueueName, queueName);
            Container.ConfigureProperty<UnicastBus.Bus>(x => x.WorkersCount, workersCount);
            Container.ConfigureProperty<UnicastBus.Bus>(x => x.Retries, retries);
            var bus = Container.Resolve<IBusInstance>();
            Container.ParentContainer.ConfigureInstance(bus);
            return bus;
        }

        public Configure RouteMessagesFromNamespaceTo<T>(string address)
        {
            Address targetAddress = Address.Parse(address);
            string ns = typeof (T).Namespace;
            IEnumerable<Type> types = AssemblyScanner.GetScannableAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(type => type.Namespace != null && type.Namespace.Equals(ns, StringComparison.InvariantCultureIgnoreCase));
            _messageRoutingTable.RouteTypes(types, targetAddress);
            return this;
        }

        private void ApplyDefaultHandlerMapping()
        {
            List<MethodInfo> methodInfos = AssemblyScanner.GetTypes().SelectMany(x => x.GetMethods().Where(method => method.ReflectedType != null && !method.IsAbstract && method.Name.Equals("Handle"))).ToList();
            MapHandlers(methodInfos);
        }

        private void MapHandlers(IEnumerable<MethodInfo> methodInfos)
        {
            foreach (MethodInfo methodInfo in methodInfos)
            {
                ParameterInfo firstParameter = methodInfo.GetParameters().FirstOrDefault();
                if (firstParameter == null) continue;

                _handlerRoutingTable.RouteTypes(new[] {firstParameter.ParameterType}, methodInfo);
                MethodInfo method = _handlerRoutingTable.ResolveRouteFor(firstParameter.ParameterType);
                Container.Configure(method.DeclaringType, Lifecycle.PerWorkUnit);
                _handlersMapped = true;
            }
        }
    }
}