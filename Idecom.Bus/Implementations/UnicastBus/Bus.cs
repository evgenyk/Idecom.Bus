using System;
using System.Reflection;
using Idecom.Bus.Addressing;
using Idecom.Bus.Interfaces;
using Idecom.Bus.Interfaces.Addons.PubSub;
using Idecom.Bus.Transport;

namespace Idecom.Bus.Implementations.UnicastBus
{
    internal class Bus : IBusInstance
    {
        private readonly IContainer _container;
        private readonly IRoutingTable<MethodInfo> _handlerRoutingTable;
        private readonly IRoutingTable<Address> _mesageRoutingTable;
        private readonly IMessageSerializer _serializer;
        private readonly IInstanceCreator _instanceCreator;
        private readonly ISubscriptionDistributor _subscriptionDistributor;
        private readonly ITransport _transport;
        private bool _isStarted;
        private Address _localAddress;

        public Bus(ITransport transport, IContainer container, IRoutingTable<Address> mesageRoutingTable, IRoutingTable<MethodInfo> handlerRoutingTable, IMessageSerializer serializer, IInstanceCreator instanceCreator, ISubscriptionDistributor subscriptionDistributor)
        {
            _container = container;
            _container.Configure<CurrentMessageContext>(ComponentLifecycle.PerUnitOfWork);

            _transport = transport;
            _transport.TransportMessageReceived += TransportMessageReceived;
            _transport.TransportMessageFinished += TransportOnTransportMessageFinished;
            _mesageRoutingTable = mesageRoutingTable;
            _handlerRoutingTable = handlerRoutingTable;
            _serializer = serializer;
            _instanceCreator = instanceCreator;
            _subscriptionDistributor = subscriptionDistributor;
        }

        public int WorkersCount { get; set; }
        public string QueueName { get; set; }
        public int Retries { get; set; }


        public IMessageContext CurrentMessageContext
        {
            get { return CurrentMessageContextInternal(); }
        }

        public IBusInstance Start()
        {
            if (_transport == null)
                throw new ArgumentException("Can not create bus. Transport hasn't been provided.");
            if (_container == null)
                throw new ArgumentException("Can not create bus. Container hasn't been provided.");
            if (_serializer == null)
                throw new ArgumentException("Can not create bus. Message serializer hasn't been provided.");
            if (WorkersCount < 1)
                throw new ArgumentException("Workers count can not be less than 1");
            _isStarted = true;

            _localAddress = new Address(QueueName);
            _transport.Start(_localAddress, _mesageRoutingTable.GetDestinations(), WorkersCount, Retries, _serializer);
            return this;
        }

        public void Stop()
        {
            if (!_isStarted)
                throw new Exception("Can not stop a bus that hasn't been started.");

            _isStarted = false;
            _transport.Stop();
        }

        public void Send(object message)
        {
            ExecuteOnlyWhenStarted(() => _transport.Send(message, _localAddress, _mesageRoutingTable.ResolveRouteFor(message.GetType()), MessageIntent.Send, CurrentMessageContextInternal()));
        }

        public void SendLocal(object message)
        {
            ExecuteOnlyWhenStarted(() => _transport.Send(message, _localAddress, _localAddress, MessageIntent.Send, CurrentMessageContextInternal()));
        }

        public void Reply(object message)
        {
            if (_localAddress.Equals(CurrentMessageContext.TransportMessage.SourceAddress))
                throw new Exception("Received a message with reply address as a local queue. This can cause an infinite loop and been stopped. Queue: " + CurrentMessageContext.TransportMessage.SourceAddress);

            ExecuteOnlyWhenStarted(() => _transport.Send(message, _localAddress, CurrentMessageContext.TransportMessage.SourceAddress, MessageIntent.Send, CurrentMessageContextInternal()));
        }

        public void Raise<T>(Action<T> action)
        {
            var message = _instanceCreator.CreateInstanceOf<T>();
            action(message);
            _subscriptionDistributor.NotifySubscribers(message);
        }

        private CurrentMessageContext CurrentMessageContextInternal()
        {
            var currentMessageContext = _container.Resolve<CurrentMessageContext>();
            _container.Release(currentMessageContext);
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
                currentMessageContext = _container.Resolve<CurrentMessageContext>();
                currentMessageContext.TransportMessage = e.TransportMessage;
                currentMessageContext.Attempt = e.Attempt;
                currentMessageContext.MaxAttempts = e.MaxRetries + 1;
                var handlerMethod = _handlerRoutingTable.ResolveRouteFor(e.TransportMessage.Message.GetType());

                var handler = _container.Resolve(handlerMethod.DeclaringType);
                handlerMethod.Invoke(handler, new[] {e.TransportMessage.Message});
            }
            finally
            {
                _container.Release(currentMessageContext);
            }
        }

        private void TransportOnTransportMessageFinished(object sender, TransportMessageFinishedEventArgs transportMessageFinishedEventArgs)
        {
            foreach (Action action in CurrentMessageContextInternal().DelayedActions)
                action();
        }
    }
}