namespace Idecom.Bus.Transport.MongoDB
{
    using System;
    using Addressing;
    using global::MongoDB.Bson;
    using Interfaces;
    using Utility;

    internal class MongoTransportMessageEntity
    {
        protected MongoTransportMessageEntity()
        {
            Status = MessageProcessingStatus.AwaitingDispatch;
        }

        public MongoTransportMessageEntity(Address sourceAddress, Address targetAddress, MessageIntent intent, string serializedMessage, Type messageType)
        {
            SourceAddress = sourceAddress.ToMongoAddress();
            TargetAddress = targetAddress.ToMongoAddress();
            Intent = intent;
            SerializedMessage = serializedMessage;
            MessageType = messageType.FullName;
            SentTimeUtc = DateTime.UtcNow;
        }

        [global::MongoDB.Bson.Serialization.Attributes.BsonElementAttribute("_id")]
        public BsonObjectId Id { get; set; }

        [global::MongoDB.Bson.Serialization.Attributes.BsonElementAttribute("s")]
        public MessageProcessingStatus Status { get; set; }

        [global::MongoDB.Bson.Serialization.Attributes.BsonElementAttribute("rb")]
        public string ReceivedBy { get; set; }

        [global::MongoDB.Bson.Serialization.Attributes.BsonElementAttribute("sm")]
        public string SerializedMessage { get; set; }

        [global::MongoDB.Bson.Serialization.Attributes.BsonElementAttribute("i")]
        public MessageIntent Intent { get; set; }

        [global::MongoDB.Bson.Serialization.Attributes.BsonElementAttribute("sa")]
        public MongoAddress SourceAddress { get; set; }

        [global::MongoDB.Bson.Serialization.Attributes.BsonElementAttribute("ta")]
        public MongoAddress TargetAddress { get; set; }

        [global::MongoDB.Bson.Serialization.Attributes.BsonElementAttribute("mt")]
        public string MessageType { get; private set; }

        [global::MongoDB.Bson.Serialization.Attributes.BsonElementAttribute("st")]
        public DateTime SentTimeUtc { get; set; }

        [global::MongoDB.Bson.Serialization.Attributes.BsonElementAttribute("rt")]
        public DateTime? ReceiveTimeUtc { get; set; }

        [global::MongoDB.Bson.Serialization.Attributes.BsonElementAttribute("ft")]
        public DateTime? FailedTimeUtc { get; set; }

        [global::MongoDB.Bson.Serialization.Attributes.BsonElementAttribute("at")]
        public DateTime? AcknowledgedTimeUtc { get; set; }

        [global::MongoDB.Bson.Serialization.Attributes.BsonElementAttribute("fr")]
        public string FailureReason { get; set; }

        public TransportMessage ToTransportMessage(IMessageSerializer serializer)
        {
            var messageType = ResolveMessageType();
            var originalMessage = serializer.DeSerialize(SerializedMessage, messageType);
            var transpoortMessage = new TransportMessage(originalMessage, SourceAddress.ToAddress(), TargetAddress.ToAddress(), Intent);
            return transpoortMessage;
        }

        private Type ResolveMessageType()
        {
            var resolvedMessageType = TypeResolver.ResolveType(MessageType);

            return resolvedMessageType;
        }

        public class MongoAddress
        {
            public MongoAddress(string queue, string datacenter)
            {
                Queue = queue;
                Datacenter = datacenter;
            }

            [global::MongoDB.Bson.Serialization.Attributes.BsonElementAttribute("q")]
            public string Queue { get; set; }

            [global::MongoDB.Bson.Serialization.Attributes.BsonElementAttribute("dc")]
            public string Datacenter { get; set; }
        }
    }
}