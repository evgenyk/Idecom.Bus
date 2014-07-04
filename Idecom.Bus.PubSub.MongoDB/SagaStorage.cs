using System;
using System.Linq;
using Idecom.Bus.Addressing;
using Idecom.Bus.Implementations;
using Idecom.Bus.Interfaces;
using Idecom.Bus.Interfaces.Addons.PubSub;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Idecom.Bus.PubSub.MongoDB
{
    public class SagaStorage : ISagaStorage, IBeforeBusStarted, IBeforeBusStopped
    {
        private const string SAGA_STORAGE_COLLECTION_NAME = "SagaStorage";
        private MongoCollection<SagaStorageEntity> _collection;
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public IRoutingTable<Type> SagaRoutingTable { get; set; }
        public Address Address { get; set; }

        public void BeforeBusStarted()
        {
            _collection = new MongoClient(ConnectionString).GetServer().GetDatabase(DatabaseName).GetCollection<SagaStorageEntity>(SAGA_STORAGE_COLLECTION_NAME);
            MapSagaDataClasses();
        }

        public void BeforeBusStopped()
        {
            _collection.Database.Server.Disconnect();
        }

        public void Update(string sagaId, object sagaData)
        {
            var byIdQuery = Query<SagaStorageEntity>.EQ(x=>x.Id, GetInternalSagaId(sagaId));
            var sagaDataWithDiscriminator = sagaData.ToBsonDocument().Set("_t", sagaData.GetType().Name);

            var update = Update<SagaStorageEntity>
                .Set(e => e.Data, sagaDataWithDiscriminator)
                .Set(e => e.OwnerEndpoint, Address.ToString())
                .Set(e=>e.DateUpdated, DateTime.UtcNow)
                .SetOnInsert(e=>e.DateStarted, DateTime.UtcNow);

            _collection.Update(byIdQuery, update, UpdateFlags.Upsert, WriteConcern.Acknowledged);
        }

        public object Get(string sagaId)
        {
            var byIdQuery = Query<SagaStorageEntity>.EQ(x=>x.Id, GetInternalSagaId(sagaId));
            var result = _collection.FindOneAs<SagaStorageEntity>(byIdQuery);
            var resultOrNull = result == null ? null : result.Data;

            return resultOrNull;
        }

        private string GetInternalSagaId(string sagaId)
        {
            return sagaId + "_" + Address;
        }

        public void Close(string sagaId)
        {
            var query = Query<SagaStorageEntity>.EQ(x => x.Id, GetInternalSagaId(sagaId));
            _collection.Remove(query);
        }

        private void MapSagaDataClasses()
        {
            var destinations = SagaRoutingTable.GetDestinations().ToList();
            var sagaDataTypes = destinations.SelectMany(x => x.BaseType.GenericTypeArguments);
            foreach (var sagaDataType in sagaDataTypes) { BsonClassMap.LookupClassMap(sagaDataType); }
        }
    }
}