using Idecom.Bus.Implementations;
using Idecom.Bus.Interfaces;

namespace Idecom.Bus.PubSub.MongoDB
{
    public static class BootstrapMongoDbPubSub
    {
        public static Configure MongoPublisher(this Configure configure, string mongodbConnectionString, string databaseName)
        {
            configure.Container.Configure<SubscriptionStorage>(ComponentLifecycle.Singleton);
            configure.Container.ConfigureProperty<SubscriptionStorage>(x => x.ConnectionString, mongodbConnectionString);
            configure.Container.ConfigureProperty<SubscriptionStorage>(x => x.DatabaseName, databaseName);

            configure.Container.Configure<SagaStorage>(ComponentLifecycle.Singleton);
            configure.Container.ConfigureProperty<SagaStorage>(x => x.ConnectionString, mongodbConnectionString);
            configure.Container.ConfigureProperty<SagaStorage>(x => x.DatabaseName, databaseName);


            return configure;
        }
    }
}