namespace Idecom.Bus.PubSub.MongoDB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Addressing;
    using global::MongoDB.Driver;
    using global::MongoDB.Driver.Builders;
    using Interfaces;
    using Interfaces.Addons.PubSub;

    public class SubscriptionStorage : ISubscriptionStorage, IBeforeBusStarted, IBeforeBusStopped
    {
        private const string SUBSCRIPTION_STORAGE_COLLECTION_NAME = "SubscriptionStorage";
        private MongoCollection<SubscriptionStorageEntity> _collection;

        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }

        public void BeforeBusStarted()
        {
            _collection = new MongoClient(ConnectionString).GetServer().GetDatabase(DatabaseName).GetCollection<SubscriptionStorageEntity>(SUBSCRIPTION_STORAGE_COLLECTION_NAME);
        }

        public void BeforeBusStopped()
        {
            _collection.Database.Server.Disconnect();
        }

        public IEnumerable<Address> GetSubscribersFor<T>() where T : class
        {
            var type = typeof (T);
            var typeWithNamespace = GetTypeNameWithNamespace(type);
            var query = Query.And(Query<SubscriptionStorageEntity>.EQ(x => x.MessageType, typeWithNamespace), Query<SubscriptionStorageEntity>.EQ(x=>x.Subscribed, true));
            var subscribers = _collection.FindAs<SubscriptionStorageEntity>(query).Select(x => Address.Parse(x.SubscriberAddress)).Distinct();
            return subscribers;
        }

        public void Subscribe(Address subscriber, Type type)
        {
            var typeWithNamespace = GetTypeNameWithNamespace(type);
            var subscriberAddress = subscriber.ToString();

            var query = Query.And(Query<SubscriptionStorageEntity>.EQ(x => x.SubscriberAddress, subscriberAddress), Query<SubscriptionStorageEntity>.EQ(x => x.MessageType, typeWithNamespace));
            var update = Update<SubscriptionStorageEntity>
                .Set(x => x.SubscriberAddress, subscriberAddress)
                .Set(x => x.MessageType, typeWithNamespace)
                .Set(x => x.Subscribed, true);
            _collection.Update(query, update, UpdateFlags.Upsert, WriteConcern.Acknowledged);
        }

        public void Unsubscribe(Address subscriber, Type type)
        {
            var typeWithNamespace = GetTypeNameWithNamespace(type);
            var subscriberAddress = subscriber.ToString();
            var query = Query.And(Query<SubscriptionStorageEntity>.EQ(x => x.SubscriberAddress, subscriberAddress), Query<SubscriptionStorageEntity>.EQ(x => x.MessageType, typeWithNamespace));
            var update = Update<SubscriptionStorageEntity>
                .Set(x => x.SubscriberAddress, subscriberAddress)
                .Set(x => x.MessageType, typeWithNamespace)
                .Set(x=>x.Subscribed, false);
            _collection.Update(query, update, UpdateFlags.Upsert, WriteConcern.Acknowledged);
        }

        private static string GetTypeNameWithNamespace(Type type)
        {
            return string.Format("{0}.{1}", type.Namespace, type.Name).ToLowerInvariant();
        }
    }
}