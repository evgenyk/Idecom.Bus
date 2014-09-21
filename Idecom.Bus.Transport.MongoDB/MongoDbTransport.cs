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
        MongoDatabase _database;
        MessageReceiver _receiver;
        MessageSender _sender;
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

        public void Send(TransportMessage transportMessage, CurrentMessageContext currentMessageContext = null)
        {
            if (transportMessage.TargetAddress == null)
                throw new InvalidOperationException(string.Format("Can not send a message of type {0} as the target address could not be found. did you forget to configure routing?",
                    (transportMessage.MessageType ?? transportMessage.Message.GetType()).Name));

            if (currentMessageContext == null)
                _sender.Send(transportMessage);
            else
                currentMessageContext.DelayedSend(transportMessage);
        }

        public event EventHandler<TransportMessageReceivedEventArgs> TransportMessageReceived;
        public event EventHandler<TransportMessageFinishedEventArgs> TransportMessageFinished;

        void CreateQueues(IEnumerable<Address> targetQueues)
        {
            foreach (var collectionName in targetQueues.Select(queue => queue.ToString()))
            {
                
                if (!_database.CollectionExists(collectionName))
                    try {
                        _database.CreateCollection(collectionName);
                    }
                    catch (Exception e) {
                        Console.WriteLine("Could not create collection {0} with exception {1}", collectionName, e);
                    }
                var mongoCollection = _database.GetCollection(collectionName);
                const string dequeueIndexName = "Stataus_Id";
                if (!mongoCollection.IndexExists(dequeueIndexName))
                    mongoCollection.CreateIndex(IndexKeys<MongoTransportMessageEntity>.Ascending(x => x.Id).Ascending(x => x.Status), IndexOptions.SetName(dequeueIndexName));
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