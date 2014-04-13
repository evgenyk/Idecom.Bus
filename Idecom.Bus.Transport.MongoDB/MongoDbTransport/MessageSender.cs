using System;
using Idecom.Bus.Addressing;
using Idecom.Bus.Interfaces;
using MongoDB.Driver;

namespace Idecom.Bus.Transport.MongoDB.MongoDbTransport
{
    internal class MessageSender
    {
        private readonly MongoDatabase _database;
        private readonly IMessageSerializer _serializer;
        private bool _isStarted;

        public MessageSender(MongoDatabase database, IMessageSerializer serializer)
        {
            _database = database;
            _serializer = serializer;
            _isStarted = true;
        }

        public bool IsStarted
        {
            get { return _isStarted; }
        }

        public void Send(object message, Address sourceAddress, Address targetAddress, MessageIntent intent)
        {
            if (!_isStarted)
                throw new Exception("Can not send messages while sender stopped.");

            MongoCollection<MongoTransportMessage> targetCollection = _database.GetCollection<MongoTransportMessage>(targetAddress.ToString());
            var mongoMessage = new MongoTransportMessage(sourceAddress, targetAddress, intent, _serializer.Serialize(message), message.GetType());
            targetCollection.Insert(mongoMessage, WriteConcern.Acknowledged);
        }

        public void Stop()
        {
            _isStarted = false;
        }
    }
}