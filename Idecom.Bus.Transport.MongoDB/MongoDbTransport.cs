using System;
using System.Collections.Generic;
using System.Linq;
using Idecom.Bus.Addressing;
using Idecom.Bus.Implementations.UnicastBus;
using Idecom.Bus.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Idecom.Bus.Transport.MongoDB
{
    public class MongoDbTransport : ITransport
    {
        private MongoDatabase _database;
        private MessageReceiver _receiver;
        private MessageSender _sender;
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public IContainer Container { get; set; }

        public int WorkersCount
        {
            get { return _receiver.WorkersCount; }
        }

        public void Start(Address localAddress, IEnumerable<Address> targetQueues, int workersCount, int retries, IMessageSerializer serializer)
        {
            _database = new MongoClient(ConnectionString).GetServer().GetDatabase(DatabaseName);
            CreateQueues(targetQueues.Union(new[] {localAddress}));
            MongoCollection<MongoTransportMessage> localCollection = _database.GetCollection<MongoTransportMessage>(localAddress.ToString());

            _sender = new MessageSender(_database, serializer);
            _receiver = new MessageReceiver(this, localCollection, workersCount, retries, serializer, Container);
        }

        public void Stop()
        {
            _receiver.Stop();
            _sender.Stop();
            _database.Server.Disconnect();
        }


        public void ChangeWorkerCount(int workers)
        {
            _receiver.ChangeWorkersCount(workers);
        }

        public void Send(object message, Address sourceAddress, Address targetAddress, MessageIntent intent, CurrentMessageContext currentMessageContext)
        {
            if (currentMessageContext == null)
                _sender.Send(message, sourceAddress, targetAddress, intent);
            else
                currentMessageContext.DelayedSend(() => _sender.Send(message, sourceAddress, targetAddress, intent));
        }

        public event EventHandler<TransportMessageReceivedEventArgs> TransportMessageReceived;
        public event EventHandler<TransportMessageFinishedEventArgs> TransportMessageFinished;

        private void CreateQueues(IEnumerable<Address> targetQueues)
        {
            foreach (string collectionName in targetQueues.Select(queue => queue.ToString()))
            {
                if (!_database.CollectionExists(collectionName))
                    _database.CreateCollection(collectionName);
                MongoCollection<BsonDocument> mongoCollection = _database.GetCollection(collectionName);
                const string dequeueIndexName = "Stataus_Id";
                if (!mongoCollection.IndexExists(dequeueIndexName))
                    mongoCollection.EnsureIndex(IndexKeys<MongoTransportMessage>.Ascending(x => x.Id).Ascending(x => x.Status), IndexOptions.SetName(dequeueIndexName));
            }
        }

        public void ProcessMessageReceivedEvent(TransportMessage transportMessage, int attempt, int maxRetries)
        {
            TransportMessageReceived(this, new TransportMessageReceivedEventArgs(transportMessage, attempt, maxRetries));
        }

        public void ProcessMessageFinishedEvent(TransportMessage transportMessage)
        {
            TransportMessageFinished(this, new TransportMessageFinishedEventArgs(transportMessage));
        }
    }
}