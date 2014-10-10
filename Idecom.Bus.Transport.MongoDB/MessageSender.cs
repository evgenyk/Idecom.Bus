namespace Idecom.Bus.Transport.MongoDB
{
    using System;
    using global::MongoDB.Driver;
    using Interfaces;

    class MessageSender
    {
        readonly MongoDatabase _database;
        readonly IMessageSerializer _serializer;
        bool _isStarted;

        public MessageSender(MongoDatabase database, IMessageSerializer serializer)
        {
            _database = database;
            _serializer = serializer;
        }

        public bool IsStarted
        {
            get { return _isStarted; }
        }

        public void Start()
        {
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

            //Log.DebugFormat("Sent message :{0} headers: {1}", transportMessage.MessageType, (transportMessage.Headers.Any() ? transportMessage.Headers.Select(x => x.Value).Aggregate((a, b) => a + b) : string.Empty));

        }

        public void Stop()
        {
            _isStarted = false;
        }
    }
}