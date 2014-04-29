using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Idecom.Bus.Addressing;
using Idecom.Bus.Implementations.Internal;
using Idecom.Bus.Interfaces;
using Idecom.Bus.Interfaces.Addons.PubSub;
using Idecom.Bus.Transport;
using Idecom.Bus.Utility;

namespace Idecom.Bus.Implementations.UnicastBus
{
    internal class Bus : IBusInstance
    {
        public IContainer Container { get; set; }
        public IRoutingTable<MethodInfo> HandlerRoutingTable { get; set; }
        public IRoutingTable<Address> MessageRoutingTable { get; set; }
        public IMessageSerializer Serializer { get; set; }
        public IInstanceCreator InstanceCreator { get; set; }
        public ISubscriptionDistributor SubscriptionDistributor { get; set; }
        public ITransport Transport { get; set; }
        public Address LocalAddress { get; set; }
        public IEffectiveConfiguration EffectiveConfiguration { get; set; }

        private bool _isStarted;

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

                ApplyHandlerMapping();

                Transport.TransportMessageReceived += TransportMessageReceived;
                Transport.TransportMessageFinished += TransportOnTransportMessageFinished;

                foreach (var beforeBusStarted in Container.ResolveAll<IBeforeBusStarted>())
                {
                    beforeBusStarted.BeforeBusStarted();
                    Container.Release(beforeBusStarted);
                }

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
            ExecuteOnlyWhenStarted(() => Transport.Send(message, MessageRoutingTable.ResolveRouteFor(message.GetType()), MessageIntent.Send, CurrentMessageContextInternal()));
        }

        public void SendLocal(object message)
        {
            ExecuteOnlyWhenStarted(() => Transport.Send(message, LocalAddress, MessageIntent.Send, CurrentMessageContextInternal()));
        }

        public void Reply(object message)
        {
            if (LocalAddress.Equals(CurrentMessageContext.TransportMessage.SourceAddress))
                throw new Exception("Received a message with reply address as a local queue. This can cause an infinite loop and been stopped. Queue: " + CurrentMessageContext.TransportMessage.SourceAddress);

            ExecuteOnlyWhenStarted(() => Transport.Send(message, CurrentMessageContext.TransportMessage.SourceAddress, MessageIntent.Send, CurrentMessageContextInternal()));
        }

        public void Raise<T>(Action<T> action)
        {
            var message = InstanceCreator.CreateInstanceOf<T>();
            action(message);
            SubscriptionDistributor.NotifySubscribers(message);
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
                var handlerMethod = HandlerRoutingTable.ResolveRouteFor(e.TransportMessage.Message.GetType());

                var handler = Container.Resolve(handlerMethod.DeclaringType);
                handlerMethod.Invoke(handler, new[] { e.TransportMessage.Message });
            }
            finally
            {
                Container.Release(currentMessageContext);
            }
        }

        private void TransportOnTransportMessageFinished(object sender, TransportMessageFinishedEventArgs transportMessageFinishedEventArgs)
        {
            foreach (Action action in CurrentMessageContextInternal().DelayedActions)
                action();
        }


        private void ApplyHandlerMapping()
        {
            var allTypes = AssemblyScanner.GetTypes().ToList();

            IEnumerable<Type> events = allTypes.Where(EffectiveConfiguration.IsEvent);

            SubscriptionDistributor.SubscribeTo(events);

            var commands = allTypes.Where(EffectiveConfiguration.IsCommand);
            var eventsAndCommands = events.Union(commands).ToList();

            foreach (var mapping in EffectiveConfiguration.MessageMappings)
            {
                var types = allTypes.Where(type => type.Namespace != null && type.Namespace.Equals(mapping.Namespace, StringComparison.InvariantCultureIgnoreCase)).ToList();
                var eventsAndCommandsInANamespace = eventsAndCommands.Intersect(types).Distinct().ToList();
                var notMappedTypes = types.Except(eventsAndCommandsInANamespace);
                if (notMappedTypes.Any())
                {
                    throw new Exception("Some messages are not mapped: " + notMappedTypes.Select(x => x.Name).Aggregate((a, b) => a + ", " + b));
                }
                MessageRoutingTable.RouteTypes(eventsAndCommandsInANamespace, mapping.Address);
            }

            Func<Type, Type, bool> implementsType = (y, compareType) => y.IsGenericType && y.GetGenericTypeDefinition() == compareType;


            IEnumerable<MethodInfo> messageToHandlerMapping = allTypes.Where(x => !x.IsInterface)
                .SelectMany(type => type.GetMethods()
                    .Where(x => x.GetParameters().Select(parameter => parameter.ParameterType)
                        .Where(type.GetInterfaces().Where(intface => implementsType(intface, typeof(IHandle<>))).SelectMany(intfs => intfs.GenericTypeArguments).Contains).Any()));
            MapMessageHandlers(messageToHandlerMapping);
        }

        private void MapMessageHandlers(IEnumerable<MethodInfo> methodInfos)
        {
            foreach (MethodInfo methodInfo in methodInfos)
            {
                ParameterInfo firstParameter = methodInfo.GetParameters().FirstOrDefault();
                if (firstParameter == null) continue;

                HandlerRoutingTable.RouteTypes(new[] { firstParameter.ParameterType }, methodInfo);
                MethodInfo method = HandlerRoutingTable.ResolveRouteFor(firstParameter.ParameterType);
                Container.Configure(method.DeclaringType, ComponentLifecycle.PerUnitOfWork);
            }
        }

    }
}