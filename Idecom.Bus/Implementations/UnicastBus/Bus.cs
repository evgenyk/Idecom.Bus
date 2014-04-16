using System;
using System.Reflection;
using Idecom.Bus.Addressing;
using Idecom.Bus.Interfaces;
using Idecom.Bus.Transport;

namespace Idecom.Bus.Implementations.UnicastBus
{
    public class Bus : IBusInstance
    {
        private readonly IContainer _container;
        private readonly IRoutingTable<MethodInfo> _handlerRoutingTable;
        private readonly IRoutingTable<Address> _mesageRoutingTable;
        private readonly IMessageSerializer _serializer;
        private readonly ITransport _transport;
        private bool _isStarted;
        private Address _localAddress;

        public Bus(ITransport transport, IContainer container, IRoutingTable<Address> mesageRoutingTable, IRoutingTable<MethodInfo> handlerRoutingTable, IMessageSerializer serializer)
        {
            _container = container;
            _container.Configure<CurrentMessageContext>(Lifecycle.PerWorkUnit);

            _transport = transport;
            _transport.TransportMessageReceived += TransportTransportMessageReceived;
            _transport.TransportMessageFinished += TransportOnTransportMessageFinished;
            _mesageRoutingTable = mesageRoutingTable;
            _handlerRoutingTable = handlerRoutingTable;
            _serializer = serializer;
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
                throw new Exception("Can not create bus. Transport hasn't been provided.");
            if (_container == null)
                throw new Exception("Can not create bus. Container hasn't been provided.");
            if (_serializer == null)
                throw new Exception("Can not create bus. Message serializer hasn't been provided.");
            if (WorkersCount < 1)
                throw new Exception("Workers count can not be less than 1");
            _isStarted = true;

            _localAddress = new Address(QueueName);
            _transport.Start(_localAddress, _mesageRoutingTable.GetDestinations(), WorkersCount, Retries, _serializer);
            return this;
        }

        public void Stop()
        {
            if (!_isStarted)
            {
                throw new Exception("Can not stop a bus that hasn't been started.");
            }
            _isStarted = false;
            _transport.Stop();
        }

        /// <summary>
        /// TODO: if send from handler we need to senbd after handler finished processing to cater for exception
        /// </summary>
        /// <param name="message"></param>
        public void Send(object message)
        {
            ExecuteOnlyWhenStarted(() => _transport.Send(message, _localAddress, _mesageRoutingTable.ResolveRouteFor(message.GetType()), MessageIntent.Send, CurrentMessageContextInternal()));
        }

        /// <summary>
        /// TODO: if send from handler we need to senbd after handler finished processing to cater for exception
        /// </summary>
        /// <param name="message"></param>
        public void SendLocal(object message)
        {
            ExecuteOnlyWhenStarted(() => _transport.Send(message, _localAddress, _localAddress, MessageIntent.Send, CurrentMessageContextInternal()));
        }

        /// <summary>
        /// TODO: if send from handler we need to senbd after handler finished processing to cater for exception
        /// </summary>
        /// <param name="message"></param>
        public void Reply(object message)
        {
            ExecuteOnlyWhenStarted(() =>
            {
                if (_localAddress.Equals(CurrentMessageContext.TransportMessage.SourceAddress))
                    throw new Exception("Can not reply to the local queue.");
                _transport.Send(message, _localAddress, CurrentMessageContext.TransportMessage.SourceAddress, MessageIntent.Send, CurrentMessageContextInternal());
            });
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

        private void TransportTransportMessageReceived(object sender, TransportMessageReceivedEventArgs e)
        {
            var currentMessageContext = _container.Resolve<CurrentMessageContext>();
            currentMessageContext.TransportMessage = e.TransportMessage;
            currentMessageContext.Attempt = e.Attempt;
            currentMessageContext.MaxAttempts = e.MaxRetries + 1; //as first message receive is not a retry and interface states "MaxAttempts", not MaxRetries
            MethodInfo handlerMethod = _handlerRoutingTable.ResolveRouteFor(e.TransportMessage.Message.GetType());

            object handler = _container.Resolve(handlerMethod.DeclaringType);
            handlerMethod.Invoke(handler, new[] {e.TransportMessage.Message});

            _container.Release(currentMessageContext);
        }

        /// <summary>
        ///     Sending messages only after current finished processing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="transportMessageFinishedEventArgs"></param>
        private void TransportOnTransportMessageFinished(object sender, TransportMessageFinishedEventArgs transportMessageFinishedEventArgs)
        {
            foreach (Action action in CurrentMessageContextInternal().DelayedActions)
                action();
        }
    }
}