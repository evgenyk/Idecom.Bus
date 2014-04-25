using System;
using System.Threading;
using System.Threading.Tasks;
using Idecom.Bus.Interfaces;
using Idecom.Bus.Utility;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Idecom.Bus.Transport.MongoDB
{
    internal class MessageReceiver
    {
        private readonly IContainer _container;
        private readonly MongoCollection<MongoTransportMessage> _localCollection;
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
            Start();
        }

        public MessageReceiver(MongoDbTransport transport, MongoCollection<MongoTransportMessage> localCollection, int workersCount, int retries, IMessageSerializer serializer, IContainer container)
            : this(workersCount, serializer)
        {
            WorkersCount = workersCount;
            _transport = transport;
            _localCollection = localCollection;
            _retries = retries;
            _container = container;

            ReturnUnfinishedMessagesToQueue(ApplicationIdGenerator.GenerateIdId());
        }

        public int WorkersCount { get; private set; }

        private void Start()
        {
            _stopReaderThread = false;
            _scheduler = new MaxWorkersTaskScheduler(_workersCount);
            _queueReaderThread = new Thread(() =>
            {
                int lastEmptyQueueSleepMs = 5;
                while (!_stopReaderThread)
                {
                    while (_scheduler.TasksPending > 0)
                        Thread.Sleep(2); //All workers are busy

                    MongoTransportMessage mongoTransportMessage = ReceiveTransportMessageFromQueue();
                    if (mongoTransportMessage == null)
                    {
                        lastEmptyQueueSleepMs += 20;
                        if (lastEmptyQueueSleepMs > 2000)
                            lastEmptyQueueSleepMs = 2000; //max sleep time is 2 seconds between pooling requests (for services which do nothing most of the time)
                        Thread.Sleep(lastEmptyQueueSleepMs); //Sleeping for a bit as queue seem to be empty and pooling would generate too much unnecessary load on the server
                        continue;
                    }
                    lastEmptyQueueSleepMs = 5;
                    new Task(() =>
                    {
                        using (_container.BeginUnitOfWork())
                        {
                            ProcessWithRetry(mongoTransportMessage.ToTransportMessage(_serializer), mongoTransportMessage);
                        }
                    }).Start(_scheduler);
                }
            });
            _queueReaderThread.Start();
        }


        private void ProcessWithRetry(TransportMessage transportMessage, MongoTransportMessage mongoTransportMessage)
        {
            int attempt = 0;

            while (attempt < _retries + 1)
            {
                try
                {
                    attempt++;
                    _transport.ProcessMessageReceivedEvent(transportMessage, attempt, _retries);
                    AchknowledgeMessageProcessed(mongoTransportMessage);
                    break;
                }
                catch (Exception exception)
                {
                    if (attempt == _retries + 1)
                        FailMessage(mongoTransportMessage, exception);
                }
            }
            //sending messages after current message been handled only.
            _transport.ProcessMessageFinishedEvent(transportMessage);
        }

        private void FailMessage(MongoTransportMessage mongoTransportMessage, Exception exception)
        {
            while (exception.InnerException != null)
                exception = exception.InnerException;
            IMongoQuery query = Query<MongoTransportMessage>.EQ(x => x.Id, mongoTransportMessage.Id);
            UpdateBuilder<MongoTransportMessage> update = Update<MongoTransportMessage>.Set(x => x.FailedTimeUtc, DateTime.UtcNow).Set(x => x.Status, MessageProcessingStatus.PermanentlyFailed).Set(x => x.FailureReason, exception.Message);
            _localCollection.Update(query, update, UpdateFlags.Multi, WriteConcern.Acknowledged);
        }

        private void AchknowledgeMessageProcessed(MongoTransportMessage mongoTransportMessage)
        {
            IMongoQuery query = Query<MongoTransportMessage>.EQ(x => x.Id, mongoTransportMessage.Id);
            _localCollection.Remove(query, WriteConcern.Acknowledged);
        }

        private MongoTransportMessage ReceiveTransportMessageFromQueue()
        {
            IMongoQuery query = Query<MongoTransportMessage>.EQ(x => x.Status, MessageProcessingStatus.AwaitingDispatch);
            UpdateBuilder<MongoTransportMessage> update = Update<MongoTransportMessage>.Set(x => x.Status, MessageProcessingStatus.ReceivedByConsumer).Set(x => x.ReceivedBy, ApplicationIdGenerator.GenerateIdId()).Set(x => x.ReceiveTimeUtc, DateTime.UtcNow);

            FindAndModifyResult transportMessages = _localCollection.FindAndModify(query, SortBy.Null, update, true);

            var transportMessage = transportMessages.GetModifiedDocumentAs<MongoTransportMessage>();
            return transportMessage;
        }

        /// <summary>
        ///     Returns the unacknowledged messages to the queue as the bus either crashed or the app has been restarted
        /// </summary>
        /// <param name="machineId"></param>
        private void ReturnUnfinishedMessagesToQueue(string machineId)
        {
            IMongoQuery query = Query.And(Query<MongoTransportMessage>.EQ(x => x.ReceivedBy, machineId), Query<MongoTransportMessage>.EQ(x => x.Status, MessageProcessingStatus.ReceivedByConsumer));
            UpdateBuilder<MongoTransportMessage> update = Update<MongoTransportMessage>.Set(x => x.ReceivedBy, null).Set(x => x.Status, MessageProcessingStatus.AwaitingDispatch).Set(x => x.ReceiveTimeUtc, null);
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