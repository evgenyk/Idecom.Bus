using System;
using Idecom.Bus.Interfaces;
using Idecom.Bus.Interfaces.Addons.PubSub;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Idecom.Bus.PubSub.MongoDB
{
    public class SagaStorage : ISagaStorage, IBeforeBusStarted, IBeforeBusStopped
    {
        private const string SAGA_STORAGE_COLLECTION_NAME = "SagaStorage";
        private MongoCollection<SubscriptionStorageEntity> _collection;
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }

        public void BeforeBusStarted()
        {
            _collection = new MongoClient(ConnectionString).GetServer().GetDatabase(DatabaseName).GetCollection<SubscriptionStorageEntity>(SAGA_STORAGE_COLLECTION_NAME);
        }

        public void BeforeBusStopped()
        {
            _collection.Database.Server.Disconnect();
        }

        void ISagaStorage.Update(string sagaId, object sagaData)
        {
            var query = Query<SagaStorageEntity>.EQ(x => x.Id, sagaId);
            var update = Update<SagaStorageEntity>
                .Set(x => x.Data, sagaData);
            var setType = Update.Set("d._t", sagaData.GetType().Name);


            var bulkOperation = _collection.InitializeOrderedBulkOperation();
            bulkOperation.Find(query).Upsert().UpdateOne(update);
            bulkOperation.Find(query).Upsert().UpdateOne(setType);

            bulkOperation.Execute(WriteConcern.Acknowledged);
        }

        public object Get(string sagaId)
        {
            var result = _collection.FindOneByIdAs<SagaStorageEntity>(sagaId);
            return null;
        }

        private static string GetTypeNameWithNamespace(Type type)
        {
            return string.Format("{0}.{1}", type.Namespace, type.Name);
        }
    }
}