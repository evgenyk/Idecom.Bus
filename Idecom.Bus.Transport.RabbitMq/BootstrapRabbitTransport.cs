namespace Idecom.Bus.Transport.RabbitMq
{
    using System;
    using System.Collections.Generic;
    using Addressing;
    using Implementations;
    using Interfaces;
    using Interfaces.Addons.PubSub;

    public class RabbitMqExchangeDistributor : ISubscriptionDistributor
    {
        public RabbitMqExchangeDistributor(ITransport transport, Address localAddress)
        {
            Transport = transport;
            LocalAddress = localAddress;
        }

        ITransport Transport { get; set; }
        Address LocalAddress { get; set; }

        public void NotifySubscribersOf(Type messageType, object message, bool isProcessingIncommingMessage, Action<TransportMessage> delayMessageAction)
        {
            var transportMessage = new TransportMessage(message, LocalAddress, null, MessageIntent.Publish, messageType);
            Transport.Send(transportMessage, isProcessingIncommingMessage, delayMessageAction);
        }

        public void SubscribeTo(IEnumerable<Type> events)
        {
        }

        public void Unsubscribe(IEnumerable<Type> events)
        {
        }
    }


    public static class BootstrapRabbitTransport
    {
        public static Configure RabbitMqTransport(this Configure configure, string rabbitmqHost, int workersCount = 1, int retries = 1)
        {
            configure.Container.Configure<RabbitMqTransport>(ComponentLifecycle.Singleton);
            configure.Container.ConfigureProperty<RabbitMqTransport>(x => x.RabbitHost, rabbitmqHost);
            configure.Container.Configure<RabbitMqExchangeDistributor>(ComponentLifecycle.Singleton);

            return configure;
        }
    }
}