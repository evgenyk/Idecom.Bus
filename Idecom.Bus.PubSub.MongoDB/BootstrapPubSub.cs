namespace Idecom.Bus.PubSub.MongoDB
{
    using Implementations;
    using Interfaces;

    public static class BootstrapPubSub
    {
        public static Configure PubSub(this Configure configure, string mongodbConnectionString, string databaseName)
        {
            configure.Container.Configure<SubscriptionStorage>(ComponentLifecycle.Singleton);
            configure.Container.ConfigureProperty<SubscriptionStorage>(x => x.ConnectionString, mongodbConnectionString);
            configure.Container.ConfigureProperty<SubscriptionStorage>(x => x.DatabaseName, databaseName);

            return configure;
        }
    }
}