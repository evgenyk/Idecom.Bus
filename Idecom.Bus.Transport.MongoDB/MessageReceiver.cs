namespace Idecom.Bus.Transport.MongoDB
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using global::MongoDB.Driver;
    using global::MongoDB.Driver.Builders;
    using Interfaces;
    using Utility;

    internal class MessageReceiver
    {
        private readonly IContainer _container;
        private readonly MongoCollection<MongoTransportMessageEntity> _localCollection;
        private readonly int _retries;
        private readonly IMessageSerializer _serializer;
        private readonly MongoDbTransport _transport;
        private Thread _queueReaderThread;
        private MaxWorkersTaskScheduler _scheduler;
        private bool _stopReaderThread;
        private int _workersCount;

        public MessageReceiver(int workersCount, IMessageSerializer serializer)
        {
            _workersCount = workersCount;
            _serializer = serializer;
        }

        public MessageReceiver(MongoDbTransport transport, MongoCollection<MongoTransportMessageEntity> localCollection, int workersCount, int retries, IMessageSerializer serializer, IContainer container)
            : this(workersCount, serializer)
        {
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

            ReturnUnfinishedMessagesToQueue(ApplicationIdGenerator.GenerateIdId());
            
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
                        Thread.Sleep(lastEmptyQueueSleepMs); //Sleeping for a bit as queue seem to be empty and pooling would generate too much unnecessary load on the server
                        continue;
                    }
                    lastEmptyQueueSleepMs = 5;
                    new Task(() => { using (_container.BeginUnitOfWork()) { ProcessWithRetry(mongoTransportMessageEntity.ToTransportMessage(_serializer), mongoTransportMessageEntity); } }).Start(_scheduler);
                }
            });
            _queueReaderThread.Start();
        }


        private void ProcessWithRetry(TransportMessage transportMessage, MongoTransportMessageEntity mongoTransportMessageEntity)
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
                        FailMessage(mongoTransportMessageEntity, exception);
                }
            }
            //sending messages after current message been handled only.
            _transport.ProcessMessageFinishedEvent(transportMessage);
        }

        private void FailMessage(MongoTransportMessageEntity mongoTransportMessageEntity, Exception exception)
        {
            while (exception.InnerException != null)
                exception = exception.InnerException;
            var query = Query<MongoTransportMessageEntity>.EQ(x => x.Id, mongoTransportMessageEntity.Id);
            var update = Update<MongoTransportMessageEntity>.Set(x => x.FailedTimeUtc, DateTime.UtcNow).Set(x => x.Status, MessageProcessingStatus.PermanentlyFailed).Set(x => x.FailureReason, exception.Message);
            _localCollection.Update(query, update, UpdateFlags.Multi, WriteConcern.Acknowledged);
        }

        private void AchknowledgeMessageProcessed(MongoTransportMessageEntity mongoTransportMessageEntity)
        {
            var query = Query<MongoTransportMessageEntity>.EQ(x => x.Id, mongoTransportMessageEntity.Id);
            _localCollection.Remove(query, WriteConcern.Acknowledged);
        }

        private MongoTransportMessageEntity ReceiveTransportMessageFromQueue()
        {
            var query = Query<MongoTransportMessageEntity>.EQ(x => x.Status, MessageProcessingStatus.AwaitingDispatch);
            var update = Update<MongoTransportMessageEntity>.Set(x => x.Status, MessageProcessingStatus.ReceivedByConsumer).Set(x => x.ReceivedBy, ApplicationIdGenerator.GenerateIdId()).Set(x => x.ReceiveTimeUtc, DateTime.UtcNow);

            var transportMessages = _localCollection.FindAndModify(query, SortBy.Null, update, true);

            var transportMessage = transportMessages.GetModifiedDocumentAs<MongoTransportMessageEntity>();
            return transportMessage;
        }

        /// <summary>
        ///     Returns the unacknowledged messages to the queue as the bus either crashed or the app has been restarted
        /// </summary>
        /// <param name="machineId"></param>
        private void ReturnUnfinishedMessagesToQueue(string machineId)
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

        public void ChangeWorkersCount(int workers)
        {
            Stop();
            _workersCount = workers;
            Start();
        }
    }
}