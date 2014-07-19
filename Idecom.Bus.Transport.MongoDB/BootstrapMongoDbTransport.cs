using Idecom.Bus.Implementations;
using Idecom.Bus.Interfaces;

namespace Idecom.Bus.Transport.MongoDB
{
    public static class BootstrapMongoDbTransport
    {
        public static Configure MongoDbTransport(this Configure configure, string mongodbConnectionString, string databaseName, int workersCount = 1, int retries = 1)
        {
            configure.Container.Configure<MongoDbTransport>(ComponentLifecycle.Singleton);
            configure.Container.ConfigureProperty<MongoDbTransport>(x => x.ConnectionString, mongodbConnectionString);
            configure.Container.ConfigureProperty<MongoDbTransport>(x => x.WorkersCount, workersCount);
            configure.Container.ConfigureProperty<MongoDbTransport>(x => x.DatabaseName, databaseName);
            configure.Container.ConfigureProperty<MongoDbTransport>(x => x.DatabaseName, databaseName);
            configure.Container.ConfigureProperty<MongoDbTransport>(x => x.Retries, retries);

            return configure;
        }
    }
}