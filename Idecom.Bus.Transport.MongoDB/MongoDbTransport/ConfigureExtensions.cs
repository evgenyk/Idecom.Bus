using Idecom.Bus.Implementations;
using Idecom.Bus.Interfaces;

namespace Idecom.Bus.Transport.MongoDB.MongoDbTransport
{
    public static class ConfigureExtensions
    {
        public static Configure MongoDbTransport(this Configure configure, string mongodbConnectionString, string databaseName)
        {
            configure.Container.Configure<MongoDbTransport>(Lifecycle.Singleton); //new MongoDbTransport(mongodbConnectionString, databaseName
            configure.Container.ConfigureProperty<MongoDbTransport>(x => x.ConnectionString, mongodbConnectionString);
            configure.Container.ConfigureProperty<MongoDbTransport>(x => x.DatabaseName, databaseName);
            return configure;
        }
    }
}