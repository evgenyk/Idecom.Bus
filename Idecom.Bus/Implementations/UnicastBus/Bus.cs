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
        public IContainer Container { get; set; }
        public IRoutingTable<MethodInfo> HandlerRoutingTable { get; set; }
        public IRoutingTable<Address> MesageRoutingTable { get; set; }
        public IMessageSerializer Serializer { get; set; }
        public IInstanceCreator InstanceCreator { get; set; }
        public ISubscriptionDistributor SubscriptionDistributor { get; set; }
        public ITransport Transport { get; set; }
        public Address LocalAddress { get; set; }

        
        
        private bool _isStarted;

        public Bus()
        {
        }

        public IMessageContext CurrentMessageContext
        {
            get { return CurrentMessageContextInternal(); }
        }

        public IBusInstance Start()
        {
            if (Transport == null)
                throw new ArgumentException("Can not create bus. Transport hasn't been provided.");
            if (Container == null)
                throw new ArgumentException("Can not create bus. Container hasn't been provided.");
            if (Serializer == null)
                throw new ArgumentException("Can not create bus. Message serializer hasn't been provided.");
            _isStarted = true;

            Transport.TransportMessageReceived += TransportMessageReceived;
            Transport.TransportMessageFinished += TransportOnTransportMessageFinished;


            foreach (IBeforeBusStarted beforeBusStarted in Container.ResolveAll<IBeforeBusStarted>())
            {
                beforeBusStarted.BeforeBusStarted();
                Container.Release(beforeBusStarted);
            }
            return this;
        }

        public void Stop()
        {
            if (!_isStarted)
                throw new Exception("Can not stop a bus that hasn't been started.");

            _isStarted = false;
            foreach (IBeforeBusStopped beforeBusStopped in Container.ResolveAll<IBeforeBusStopped>())
            {
                beforeBusStopped.BeforeBusStopped();
                Container.Release(beforeBusStopped);
            }
        }

        public void Send(object message)
        {
            ExecuteOnlyWhenStarted(() => Transport.Send(message, MesageRoutingTable.ResolveRouteFor(message.GetType()), MessageIntent.Send, CurrentMessageContextInternal()));
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
                handlerMethod.Invoke(handler, new[] {e.TransportMessage.Message});
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
    }
}