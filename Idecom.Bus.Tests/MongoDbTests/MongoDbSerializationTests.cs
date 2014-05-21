using Idecom.Bus.PubSub.MongoDB;
using MongoDB.Driver;
using Xunit;

namespace Idecom.Bus.Tests.MongoDbTests
{
    public class MongoDbSerializationTests
    {
        [Fact]
        public void CanSerialiaeADocumentWithPolymorphicPropertyTest()
        {
            var connectionString = "mongodb://localhost";
            var databaseName = "SerializerTest";
            string collectionName = "SerializerTestCollection";
            var _collection = new MongoClient(connectionString).GetServer().GetDatabase(databaseName).GetCollection<SubscriptionStorageEntity>(collectionName);
        }
    }
}