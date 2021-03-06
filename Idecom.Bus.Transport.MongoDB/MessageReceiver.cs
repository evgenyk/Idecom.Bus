namespace Idecom.Bus.Transport.MongoDB
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Addressing;
    using global::MongoDB.Driver;
    using global::MongoDB.Driver.Builders;
    using Interfaces;
    using Interfaces.Logging;
    using Utility;

    class MessageReceiver
    {
        readonly IContainer _container;
        readonly MongoCollection<MongoTransportMessageEntity> _localCollection;
        readonly int _retries;
        readonly IMessageSerializer _serializer;
        readonly MongoDbTransport _transport;
        Thread _queueReaderThread;
        MaxWorkersTaskScheduler _scheduler;
        bool _stopReaderThread;
        readonly int _workersCount;
        ILog _log;

        public MessageReceiver(int workersCount, IMessageSerializer serializer)
        {
            _workersCount = workersCount;
            _serializer = serializer;
        }

        public MessageReceiver(MongoDbTransport transport,
                               MongoCollection<MongoTransportMessageEntity> localCollection,
                               int workersCount,
                               int retries,
                               IMessageSerializer serializer,
                               IContainer container, ILogFactory logFactory, Address address)
            : this(workersCount, serializer)
        {
            _log = logFactory.GetLogger(string.Format("{0} MessageSender", address));
            WorkersCount = workersCount;
            _transport = transport;
            _localCollection = localCollection;
            _retries = retries;
            _container = container;
        }

        public int WorkersCount { get; private set; }

        public void Start()
        {
            _stopReaderThread = false;

            ReturnUnfinishedMessagesToQueue(ApplicationIdGenerator.GenerateId());

            _scheduler = new MaxWorkersTaskScheduler(_workersCount);
            _queueReaderThread = new Thread(() =>
                                            {
                                                var lastEmptyQueueSleepMs = 5;
                                                while (!_stopReaderThread)
                                                {
                                                    while (_scheduler.TasksPending > 0)
                                                        Thread.Sleep(2); //All workers are busy


                                                    var mongoTransportMessageEntity = ReceiveTransportMessageFromQueue();

                                                    if (mongoTransportMessageEntity == null)
                                                    {
                                                        lastEmptyQueueSleepMs += 20;
                                                        if (lastEmptyQueueSleepMs > 2000)
                                                            lastEmptyQueueSleepMs = 2000; //max sleep time is 2 seconds between pooling requests (for services which do nothing most of the time)
                                                        Thread.Sleep(lastEmptyQueueSleepMs);
                                                        //Sleeping for a bit as queue seem to be empty and pooling would generate too much unnecessary load on the server
                                                        continue;
                                                    }
                                                    lastEmptyQueueSleepMs = 5;
                                                    new Task(
                                                        () =>
                                                        {
                                                            using (_container.BeginUnitOfWork())
                                                            {
                                                                ProcessWithRetry(mongoTransportMessageEntity.ToTransportMessage(_serializer), mongoTransportMessageEntity);
                                                            }
                                                        }, TaskCreationOptions.LongRunning).Start(_scheduler);
                                                }
                                            });
            _queueReaderThread.Start();
        }


        void ProcessWithRetry(TransportMessage transportMessage, MongoTransportMessageEntity mongoTransportMessageEntity)
        {
            var attempt = 0;

            while (attempt < _retries + 1)
            {
                try
                {
                    attempt++;
                    _transport.ProcessMessageReceivedEvent(transportMessage, attempt, _retries);
                    AchknowledgeMessageProcessed(mongoTransportMessageEntity);
                    break;
                }
                catch (Exception exception)
                {
                    if (attempt == _retries + 1)
                        FailMessage(mongoTransportMessageEntity, exception, attempt);
                }
            }
        }

        void FailMessage(MongoTransportMessageEntity mongoTransportMessageEntity, Exception exception, int attempt)
        {
            while (exception.InnerException != null)
                exception = exception.InnerException;
            var query = Query<MongoTransportMessageEntity>.EQ(x => x.Id, mongoTransportMessageEntity.Id);
            var update = Update<MongoTransportMessageEntity>.Set(x => x.FailedTimeUtc, DateTime.UtcNow)
                                                            .Set(x => x.Status, MessageProcessingStatus.PermanentlyFailed)
                                                            .Set(x => x.FailureReason, string.Format("{0}[{1}]", exception.Message, exception.StackTrace));
            _localCollection.Update(query, update, UpdateFlags.Multi, WriteConcern.Acknowledged);

            _log.WarnFormat("Message {0} failed processing {1} times and is moved to the error queue with exception \"{2}\"", mongoTransportMessageEntity.MessageType, attempt + 1, exception.Message);
        }

        void AchknowledgeMessageProcessed(MongoTransportMessageEntity mongoTransportMessageEntity)
        {
            var query = Query<MongoTransportMessageEntity>.EQ(x => x.Id, mongoTransportMessageEntity.Id);
            _localCollection.Remove(query, WriteConcern.Acknowledged);
        }

        MongoTransportMessageEntity ReceiveTransportMessageFromQueue()
        {
            var args = new FindAndModifyArgs()
                       {
                           Query = Query<MongoTransportMessageEntity>.EQ(x => x.Status, MessageProcessingStatus.AwaitingDispatch),
                           Update = Update<MongoTransportMessageEntity>.Set(x => x.Status, MessageProcessingStatus.ReceivedByConsumer)
                                                                       .Set(x => x.ReceivedBy, ApplicationIdGenerator.GenerateId())
                                                                       .Set(x => x.ReceiveTimeUtc, DateTime.UtcNow),
                           SortBy = SortBy<MongoTransportMessageEntity>.Ascending(entity => entity.Id),
                           Upsert = false,
                       };

            var transportMessages = _localCollection.FindAndModify(args);

            var transportMessage = transportMessages.GetModifiedDocumentAs<MongoTransportMessageEntity>();
            return transportMessage;
        }

        /// <summary>
        ///     Returns the unacknowledged messages to the queue as the bus either crashed or the app has been restarted
        /// </summary>
        /// <param name="machineId"></param>
        void ReturnUnfinishedMessagesToQueue(string machineId)
        {
            var query = Query.And(Query<MongoTransportMessageEntity>.EQ(x => x.ReceivedBy, machineId), Query<MongoTransportMessageEntity>.EQ(x => x.Status, MessageProcessingStatus.ReceivedByConsumer));
            var update = Update<MongoTransportMessageEntity>.Set(x => x.ReceivedBy, null).Set(x => x.Status, MessageProcessingStatus.AwaitingDispatch).Set(x => x.ReceiveTimeUtc, null);
            _localCollection.Update(query, update, UpdateFlags.Multi, WriteConcern.Acknowledged);
        }


        public void Stop()
        {
            _stopReaderThread = true;
            _queueReaderThread.Join();
            _scheduler.Dispose();
        }
    }
}