using System.Collections.Generic;

namespace Idecom.Bus.Transport.MongoDB
{
    using System;
    using Addressing;
    using global::MongoDB.Driver;
    using Interfaces;

    internal class MessageSender
    {
        private readonly MongoDatabase _database;
        private readonly IMessageSerializer _serializer;
        private bool _isStarted;

        public MessageSender(MongoDatabase database, IMessageSerializer serializer)
        {
            _database = database;
            _serializer = serializer;
        }

        public void Start()
        {
            _isStarted = true;
        }

        public bool IsStarted
        {
            get { return _isStarted; }
        }

        public void Send(TransportMessage transportMessage)
        {
            if (!_isStarted)
                throw new Exception("Can not send messages while sender stopped.");

            var targetCollection = _database.GetCollection<MongoTransportMessageEntity>(transportMessage.TargetAddress.ToString());
            var type = transportMessage.MessageType ?? transportMessage.Message.GetType();


            var mongoMessage = new MongoTransportMessageEntity(transportMessage.SourceAddress, transportMessage.TargetAddress, transportMessage.Intent, _serializer.Serialize(transportMessage.Message), type, transportMessage.Headers);
            targetCollection.Insert(mongoMessage, WriteConcern.Acknowledged);
        }

        public void Stop()
        {
            _isStarted = false;
        }
    }
}