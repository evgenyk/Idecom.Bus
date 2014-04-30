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
            _isStarted = true;
        }

        public bool IsStarted
        {
            get { return _isStarted; }
        }

        public void Send(object message, Address sourceAddress, Address targetAddress, MessageIntent intent, Type messageType)
        {
            if (!_isStarted)
                throw new Exception("Can not send messages while sender stopped.");

            var targetCollection = _database.GetCollection<MongoTransportMessageEntity>(targetAddress.ToString());
            var type = messageType ?? message.GetType();

            var mongoMessage = new MongoTransportMessageEntity(sourceAddress, targetAddress, intent, _serializer.Serialize(message), type);
            targetCollection.Insert(mongoMessage, WriteConcern.Acknowledged);
        }

        public void Stop()
        {
            _isStarted = false;
        }
    }
}