namespace Idecom.Bus.Transport.MongoDB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Addressing;
    using global::MongoDB.Driver;
    using global::MongoDB.Driver.Builders;
    using Implementations;
    using Implementations.UnicastBus;
    using Interfaces;

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

        public int Retries { get; set; }

        public void BeforeBusStarted()
        {
            _database = new MongoClient(ConnectionString).GetServer().GetDatabase(DatabaseName);
            CreateQueues(MesageRoutingTable.GetDestinations().Union(new[] {LocalAddress}));
            var localCollection = _database.GetCollection<MongoTransportMessageEntity>(LocalAddress.ToString());

            _sender = new MessageSender(_database, MessageSerializer);
            _sender.Start();
            _receiver = new MessageReceiver(this, localCollection, WorkersCount, Retries, MessageSerializer, Container);
            _receiver.Start();
        }

        public void BeforeBusStopped()
        {
            _receiver.Stop();
            _sender.Stop();
            _database.Server.Disconnect();
        }

        public int WorkersCount { get; set; }

        public void ChangeWorkerCount(int workers)
        {
            _receiver.ChangeWorkersCount(workers);
        }

        public void Send(object message, Address targetAddress, MessageIntent intent, CurrentMessageContext currentMessageContext, Type type)
        {
            if (currentMessageContext == null)
                _sender.Send(message, LocalAddress, targetAddress, intent, type);
            else
                currentMessageContext.DelayedSend(message, LocalAddress, targetAddress, intent, type);
        }

        public event EventHandler<TransportMessageReceivedEventArgs> TransportMessageReceived;
        public event EventHandler<TransportMessageFinishedEventArgs> TransportMessageFinished;

        private void CreateQueues(IEnumerable<Address> targetQueues)
        {
            foreach (string collectionName in targetQueues.Select(queue => queue.ToString()))
            {
                if (!_database.CollectionExists(collectionName))
                    _database.CreateCollection(collectionName);
                var mongoCollection = _database.GetCollection(collectionName);
                const string dequeueIndexName = "Stataus_Id";
                if (!mongoCollection.IndexExists(dequeueIndexName))
                    mongoCollection.EnsureIndex(IndexKeys<MongoTransportMessageEntity>.Ascending(x => x.Id).Ascending(x => x.Status), IndexOptions.SetName(dequeueIndexName));
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

        public void AfterBusStopped()
        {
            throw new NotImplementedException();
        }
    }
}