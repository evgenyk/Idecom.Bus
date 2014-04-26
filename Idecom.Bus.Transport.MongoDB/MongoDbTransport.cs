using System;
using System.Collections.Generic;
using System.Linq;
using Idecom.Bus.Addressing;
using Idecom.Bus.Implementations;
using Idecom.Bus.Implementations.UnicastBus;
using Idecom.Bus.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Idecom.Bus.Transport.MongoDB
{
    public class MongoDbTransport : ITransport, IBeforeBusStarted, IBeforeBusStopped
    {
        private MongoDatabase _database;
        private MessageReceiver _receiver;
        private MessageSender _sender;
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public IContainer Container { get; set; }
        public IRoutingTable<Address> MesageRoutingTable { get; set; }
        public IMessageSerializer MessageSerializer { get; set; }
        public Address LocalAddress { get; set; }

        public int WorkersCount { get; set; }
        public int Retries { get; set; }

        public void ChangeWorkerCount(int workers)
        {
            _receiver.ChangeWorkersCount(workers);
        }

        public void Send(object message, Address targetAddress, MessageIntent intent, CurrentMessageContext currentMessageContext)
        {
            if (currentMessageContext == null)
                _sender.Send(message, LocalAddress, targetAddress, intent);
            else
                currentMessageContext.DelayedSend(() => _sender.Send(message, LocalAddress, targetAddress, intent));
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

        public void BeforeBusStarted()
        {
            _database = new MongoClient(ConnectionString).GetServer().GetDatabase(DatabaseName);
            CreateQueues(MesageRoutingTable.GetDestinations().Union(new[] { LocalAddress }));
            MongoCollection<MongoTransportMessage> localCollection = _database.GetCollection<MongoTransportMessage>(LocalAddress.ToString());

            _sender = new MessageSender(_database, MessageSerializer);
            _receiver = new MessageReceiver(this, localCollection, WorkersCount, Retries, MessageSerializer, Container);
        }

        public void AfterBusStopped()
        {
            throw new NotImplementedException();
        }

        public void BeforeBusStopped()
        {
            _receiver.Stop();
            _sender.Stop();
            _database.Server.Disconnect();
        }
    }
}