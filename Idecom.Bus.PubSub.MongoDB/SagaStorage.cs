using System;
using Idecom.Bus.Interfaces;
using Idecom.Bus.Interfaces.Addons.PubSub;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Idecom.Bus.PubSub.MongoDB
{
    public class SagaStorage : ISagaStorage, IBeforeBusStarted, IBeforeBusStopped
    {
        private const string SAGA_STORAGE_COLLECTION_NAME = "SagaStorage";
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        private MongoCollection<SubscriptionStorageEntity> _collection;

        public void Update(string sagaId, object sagaData)
        {
            var query = Query<SagaStorageEntity>.EQ(x=>x.Id, sagaId);
            var update = Update<SagaStorageEntity>
                .Set(x => x.Data, sagaData)
                .Set(x=>x.DataType, GetTypeNameWithNamespace(sagaData.GetType()));
            _collection.Update(query, update, UpdateFlags.Upsert, WriteConcern.Acknowledged);
        }

        public object Get(string sagaId)
        {
            var query = Query.EQ("_id", sagaId);
            var result = _collection.FindOneAs<SagaStorageEntity>(query);
            return null;
        }

        public void BeforeBusStarted()
        {
            _collection = new MongoClient(ConnectionString).GetServer().GetDatabase(DatabaseName).GetCollection<SubscriptionStorageEntity>(SAGA_STORAGE_COLLECTION_NAME);
        }

        public void BeforeBusStopped()
        {
            _collection.Database.Server.Disconnect();
        }

        private static string GetTypeNameWithNamespace(Type type)
        {
            return string.Format("{0}.{1}", type.Namespace, type.Name).ToLowerInvariant();
        }

    }
}