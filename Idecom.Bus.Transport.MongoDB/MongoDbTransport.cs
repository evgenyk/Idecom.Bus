namespace Idecom.Bus.Transport.MongoDB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Addressing;
    using global::MongoDB.Driver;
    using global::MongoDB.Driver.Builders;
    using Implementations;
    using Implementations.Behaviors;
    using Implementations.UnicastBus;
    using Interfaces;
    using Interfaces.Behaviors;
    using Interfaces.Logging;

    public class MongoDbTransport : ITransport, IBeforeBusStarted, IBeforeBusStopped
    {
        MongoDatabase _database;
        MessageReceiver _receiver;
        MessageSender _sender;
        readonly Address _localAddress;
        readonly ILogFactory _logFactory;
        readonly ILog _log;

        public MongoDbTransport(Address localAddress, ILogFactory logFactory)
        {
            _localAddress = localAddress;
            _logFactory = logFactory;
            _log = logFactory.GetLogger(string.Format("{0} MongoDbTransport", localAddress));
        }

        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public IContainer Container { get; set; }
        public IRoutingTable<Address> MesageRoutingTable { get; set; }
        public IMessageSerializer MessageSerializer { get; set; }
        public IBehaviorChains Chains { get; set; }

        public int Retries { get; set; }
        public int WorkersCount { get; set; }

        public void BeforeBusStarted()
        {
            _database = new MongoClient(ConnectionString).GetServer().GetDatabase(DatabaseName);
            CreateQueues(MesageRoutingTable.GetDestinations().Union(new[] {_localAddress}));
            var localCollection = _database.GetCollection<MongoTransportMessageEntity>(_localAddress.ToString());

            _sender = new MessageSender(_database, MessageSerializer, _logFactory, _localAddress);
            _sender.Start();
            _receiver = new MessageReceiver(this, localCollection, WorkersCount, Retries, MessageSerializer, Container, _logFactory, _localAddress);
            _receiver.Start();
        }

        public void BeforeBusStopped()
        {
            _receiver.Stop();
            _sender.Stop();
            _database.Server.Disconnect();
        }

        public void Send(TransportMessage transportMessage, bool isProcessingIncommingMessage, Action<TransportMessage> delayMessageAction)
        {
            if (isProcessingIncommingMessage)
            {
                delayMessageAction(transportMessage);
                return;
            }

            if (transportMessage.TargetAddress == null)
                throw new InvalidOperationException(string.Format("Can not send a message of type {0} as the target address could not be found. did you forget to configure routing?",
                    (transportMessage.MessageType ?? transportMessage.Message.GetType()).Name));

            _sender.Send(transportMessage);
        }

        void CreateQueues(IEnumerable<Address> targetQueues)
        {
            foreach (var collectionName in targetQueues.Select(queue => queue.ToString()))
            {
                
                if (!_database.CollectionExists(collectionName))
                    try { _database.CreateCollection(collectionName); }
                    catch (Exception e) {
                        _log.ErrorFormat("Could not create collection {0} with exception {1}", collectionName, e);
                    }
                var mongoCollection = _database.GetCollection(collectionName);
                const string dequeueIndexName = "Stataus_Id";
                if (!mongoCollection.IndexExists(dequeueIndexName))
                    try
                    {
                        var indexKeysBuilder = IndexKeys<MongoTransportMessageEntity>.Ascending(x => x.Id).Ascending(x => x.Status);
                        var indexOptionsBuilder = IndexOptions.SetName(dequeueIndexName);
                        mongoCollection.CreateIndex(indexKeysBuilder, indexOptionsBuilder);
                    }
                    catch (Exception e) {
                        _log.ErrorFormat("Could not create index {0} with exception {1}", dequeueIndexName, e);
                    }
            }
        }

        public void ProcessMessageReceivedEvent(TransportMessage transportMessage, int attempt, int maxRetries)
        {
            _log.DebugFormat("Received an incoming message of type {0}", (transportMessage.MessageType == null ? transportMessage.Message.GetType().ToString() : transportMessage.MessageType.Name));

            var ce = new ChainExecutor(Container);
            var chain = Chains.GetChainFor(ChainIntent.TransportMessageReceive);

            using (var ct = AmbientChainContext.Current.Push(context => { context.IncomingMessageContext = new IncommingMessageContext(transportMessage, attempt, maxRetries); })) { ce.RunWithIt(chain, ct); }
        }
    }
}