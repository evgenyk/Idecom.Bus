using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Idecom.Bus.Implementations;
using Idecom.Bus.Interfaces;
using Idecom.Bus.Interfaces.Addons.PubSub;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Idecom.Bus.PubSub.MongoDB
{
    public class SagaStorage : ISagaStorage, IBeforeBusStarted, IBeforeBusStopped
    {
        private const string SAGA_STORAGE_COLLECTION_NAME = "SagaStorage";
        private List<Type> KnownTypes;
        private MongoCollection<SagaStorageEntity> _collection;
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public IRoutingTable<Type> SagaRoutingTable { get; set; }

        public void BeforeBusStarted()
        {
            _collection = new MongoClient(ConnectionString).GetServer().GetDatabase(DatabaseName).GetCollection<SagaStorageEntity>(SAGA_STORAGE_COLLECTION_NAME);
            var destinations = SagaRoutingTable.GetDestinations().ToList();
            var sagaDataTypes = destinations.SelectMany(x => x.BaseType.GenericTypeArguments);
            var heh2 = BsonClassMap.GetRegisteredClassMaps();
            foreach (var sagaDataType in sagaDataTypes) { BsonClassMap.LookupClassMap(sagaDataType); }
            var heh = BsonClassMap.GetRegisteredClassMaps();

            var currentThread = Thread.CurrentThread.ManagedThreadId;
        }

        public void BeforeBusStopped()
        {
            _collection.Database.Server.Disconnect();
        }

        public void Update(string sagaId, object sagaData)
        {
            //            var lookupClassMap = BsonClassMap.LookupClassMap(sagaData.GetType());
            //            lookupClassMap.SetDiscriminatorIsRequired(true);
            //            lookupClassMap.SetDiscriminator(sagaData.GetType().Name);
            //            BsonClassMap.RegisterClassMap(lookupClassMap);

            var currentThread = Thread.CurrentThread.ManagedThreadId;
            var gg = BsonClassMap.GetRegisteredClassMaps();
            var heh = BsonClassMap.LookupClassMap(sagaData.GetType());
            _collection.Insert(new SagaStorageEntity(sagaId, sagaData));

            //            var query = Query<SagaStorageEntity>.EQ(x => x.Id, sagaId);
            //            var update = Update<SagaStorageEntity>
            //                .Set(x => x.Data, sagaData);
            //            var setType = Update.Set("d._t", sagaData.GetType().Name);
            //
            //
            //            var bulkOperation = _collection.InitializeOrderedBulkOperation();
            //            bulkOperation.Find(query).Upsert().UpdateOne(update);
            //            bulkOperation.Find(query).Upsert().UpdateOne(setType);
            //
            //            bulkOperation.Execute(WriteConcern.Acknowledged);
        }

        public object Get(string sagaId)
        {
            var currentThread = Thread.CurrentThread.ManagedThreadId;


            var heh = BsonClassMap.GetRegisteredClassMaps();
            var heh2 = _collection.FindOneById(sagaId);
            var result = _collection.FindOneByIdAs<SagaStorageEntity>(sagaId);
            return result.Data;
        }

        public void KnownSagaTypes(IEnumerable<Type> types)
        {
        }
    }
}