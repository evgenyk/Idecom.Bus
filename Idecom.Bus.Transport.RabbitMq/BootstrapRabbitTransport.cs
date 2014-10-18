namespace Idecom.Bus.Transport.RabbitMq
{
    using Implementations;
    using Interfaces;

    public static class BootstrapRabbitTransport
    {
        public static Configure RabbitMqTransport(this Configure configure, string rabbitmqHost, int workersCount = 1, int retries = 1)
        {
            configure.Container.Configure<RabbitMqTransport>(ComponentLifecycle.Singleton);
            configure.Container.ConfigureProperty<RabbitMqTransport>(x => x.RabbitHost, rabbitmqHost);

            return configure;
        }
    }
}