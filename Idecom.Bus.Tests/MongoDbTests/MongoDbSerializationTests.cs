using System;
using System.Reflection;
using Idecom.Bus.PubSub.MongoDB;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Xunit;

namespace Idecom.Bus.Tests.MongoDbTests
{
    public class MongoDbSerializationTests
    {
        static MongoDbSerializationTests()
        {
            var type = Assembly.LoadFile(@"C:\Projects\Idecom.Bus\Idecom.Bus.SampleApp1\bin\Debug\Idecom.Bus.SampleApp1.exe");
            BsonClassMap.LookupClassMap(type.GetType("Idecom.Bus.SampleApp1.Bus1Handlers.NicePersonConversationState"));
        }

        [Fact]
        public void CanSerialiaeADocumentWithPolymorphicPropertyTest()
        {
            var connectionString = "mongodb://localhost";
            var databaseName = "SerializerTest";
            string collectionName = "SerializerTestCollection";
            var collection = new MongoClient(connectionString).GetServer().GetDatabase(databaseName).GetCollection<SagaStorageEntity>(collectionName);
            string id = "d6e59597-aab9-41ac-b24c-82fe2844e4a3";

            //var type = Assembly.LoadFile(@"C:\Projects\Idecom.Bus\Idecom.Bus.SampleApp1\bin\Debug\Idecom.Bus.SampleApp1.exe").GetType("Idecom.Bus.SampleApp1.Bus1Handlers.NicePersonConversationState");
            //var instance = Activator.CreateInstance(type);
            //collection.Insert(new SagaStorageEntity(id, instance));

            var maps = BsonClassMap.GetRegisteredClassMaps();
            
            var heh = collection.FindOneByIdAs<SagaStorageEntity>(id);
        }
    }
}