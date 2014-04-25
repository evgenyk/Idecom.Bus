using Idecom.Bus.Implementations;
using Idecom.Bus.Interfaces;

namespace Idecom.Bus.Transport.MongoDB
{
    public static class BootstrapMongoDbTransport
    {
        public static Configure MongoDbTransport(this Configure configure, string mongodbConnectionString, string databaseName)
        {
            configure.Container.Configure<MongoDbTransport>(ComponentLifecycle.Singleton);
            configure.Container.ConfigureProperty<MongoDbTransport>(x => x.ConnectionString, mongodbConnectionString);
            configure.Container.ConfigureProperty<MongoDbTransport>(x => x.DatabaseName, databaseName);
            return configure;
        }
    }
}