using System;
using System.Collections.Generic;
using Idecom.Bus.Addressing;
using Idecom.Bus.Interfaces;
using Idecom.Bus.Interfaces.Addons.PubSub;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Idecom.Bus.PubSub.MongoDB
{
    public class SubscriptionStorageEntity
    {
        [BsonElement("_id")]
        public BsonObjectId Id { get; set; }

        [BsonElement("mt")]
        public string MessageType { get; set; }

        [BsonElement("se")]
        public string SubscriberAddress { get; set; }

    }
    
    public class SubscriptionStorage: ISubscriptionStorage, IBeforeBusStarted, IBeforeBusStopped
    {
        private MongoCollection<SubscriptionStorageEntity> _collection;
        private const string SUBSCRIPTION_STORAGE_COLLECTION_NAME = "SubscriptionStorage";
        
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }


        public IEnumerable<Address> GetSubscribersFor(Type eventType)
        {
            return new List<Address>();
        }

        public void Subscribe(Address subscriber, Type eventType)
        {
            _collection.Update()
        }

        public void Unsubscribe(Address subscriber, Type eventType)
        {
            throw new NotImplementedException();
        }

        public void BeforeBusStarted()
        {
            _collection = new MongoClient(ConnectionString).GetServer().GetDatabase(DatabaseName).GetCollection<SubscriptionStorageEntity>(SUBSCRIPTION_STORAGE_COLLECTION_NAME);
        }

        public void BeforeBusStopped()
        {
            
        }
    }
}