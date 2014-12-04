namespace Idecom.Bus.Transport.MongoDB
{
    using System;
    using Addressing;
    using global::MongoDB.Driver;
    using Interfaces;
    using Interfaces.Logging;

    class MessageSender
    {
        readonly MongoDatabase _database;
        readonly IMessageSerializer _serializer;
        bool _isStarted;
        ILog _log;

        public MessageSender(MongoDatabase database, IMessageSerializer serializer, ILogFactory logFactory, Address address)
        {
            _database = database;
            _serializer = serializer;
            _log = logFactory.GetLogger(string.Format("{0} MessageSender", address));
        }

        public bool IsStarted
        {
            get { return _isStarted; }
        }

        public void Start()
        {
            _log.Debug("Started");
            _isStarted = true;
        }

        public void Send(TransportMessage transportMessage)
        {
            if (!_isStarted)
                throw new Exception("Can not send messages while sender stopped.");

            var targetCollection = _database.GetCollection<MongoTransportMessageEntity>(transportMessage.TargetAddress.ToString());
            var type = transportMessage.MessageType ?? transportMessage.Message.GetType();


            var serializedMessage = _serializer.Serialize(transportMessage.Message);
            var mongoMessage = new MongoTransportMessageEntity(transportMessage.SourceAddress, transportMessage.TargetAddress, transportMessage.Intent, serializedMessage, type,
                transportMessage.Headers);
            targetCollection.Insert(mongoMessage, WriteConcern.Acknowledged);

        }

        public void Stop()
        {
            _log.Debug("Stopped");
            _isStarted = false;
        }
    }
}