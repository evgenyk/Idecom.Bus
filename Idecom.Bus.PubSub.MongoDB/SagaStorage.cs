namespace Idecom.Bus.PubSub.MongoDB
{
    using System;
    using System.Linq;
    using Addressing;
    using global::MongoDB.Bson;
    using global::MongoDB.Bson.Serialization;
    using global::MongoDB.Driver;
    using global::MongoDB.Driver.Builders;
    using Implementations;
    using Interfaces;
    using Interfaces.Addons.PubSub;

    public class SagaStorage : ISagaStorage, IBeforeBusStarted, IBeforeBusStopped
    {
        const string SAGA_STORAGE_COLLECTION_NAME = "SagaStorage";
        MongoCollection<SagaStorageEntity> _collection;
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
            var byIdQuery = Query<SagaStorageEntity>.EQ(x => x.Id, GetInternalSagaId(sagaId));
            var sagaDataWithDiscriminator = sagaData.ToBsonDocument().Set("_t", sagaData.GetType().Name);

            var update = Update<SagaStorageEntity>
                .Set(e => e.Data, sagaDataWithDiscriminator)
                .Set(e => e.OwnerEndpoint, Address.ToString())
                .Set(e => e.DateUpdated, DateTime.UtcNow)
                .SetOnInsert(e => e.DateStarted, DateTime.UtcNow);

            _collection.Update(byIdQuery, update, UpdateFlags.Upsert, WriteConcern.Acknowledged);
        }

        public object Get(string sagaId)
        {
            var byIdQuery = Query<SagaStorageEntity>.EQ(x => x.Id, GetInternalSagaId(sagaId));
            var result = _collection.FindOneAs<SagaStorageEntity>(byIdQuery);
            var resultOrNull = result == null ? null : result.Data;

            return resultOrNull;
        }

        public void Close(string sagaId)
        {
            var query = Query<SagaStorageEntity>.EQ(x => x.Id, GetInternalSagaId(sagaId));
            _collection.Remove(query);
        }

        string GetInternalSagaId(string sagaId)
        {
            return sagaId + "_" + Address;
        }

        void MapSagaDataClasses()
        {
            var destinations = SagaRoutingTable.GetDestinations().ToList();
            var sagaDataTypes = destinations.SelectMany(x => x.BaseType.GenericTypeArguments);
            foreach (var sagaDataType in sagaDataTypes) { BsonClassMap.LookupClassMap(sagaDataType); }
        }
    }
}