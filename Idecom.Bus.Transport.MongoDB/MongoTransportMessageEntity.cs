using System;
using Idecom.Bus.Addressing;
using Idecom.Bus.Interfaces;
using Idecom.Bus.Utility;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Idecom.Bus.Transport.MongoDB
{
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

        [BsonElement("_id")]
        public BsonObjectId Id { get; set; }

        [BsonElement("s")]
        public MessageProcessingStatus Status { get; set; }

        [BsonElement("rb")]
        public string ReceivedBy { get; set; }

        [BsonElement("sm")]
        public string SerializedMessage { get; set; }

        [BsonElement("i")]
        public MessageIntent Intent { get; set; }

        [BsonElement("sa")]
        public MongoAddress SourceAddress { get; set; }

        [BsonElement("ta")]
        public MongoAddress TargetAddress { get; set; }

        [BsonElement("mt")]
        public string MessageType { get; private set; }

        [BsonElement("st")]
        public DateTime SentTimeUtc { get; set; }

        [BsonElement("rt")]
        public DateTime? ReceiveTimeUtc { get; set; }

        [BsonElement("ft")]
        public DateTime? FailedTimeUtc { get; set; }

        [BsonElement("at")]
        public DateTime? AcknowledgedTimeUtc { get; set; }

        [BsonElement("fr")]
        public string FailureReason { get; set; }

        public TransportMessage ToTransportMessage(IMessageSerializer serializer)
        {
            Type messageType = ResolveMessageType();
            object originalMessage = serializer.DeSerialize(SerializedMessage, messageType);
            var transpoortMessage = new TransportMessage(originalMessage, SourceAddress.ToAddress(), TargetAddress.ToAddress(), Intent);
            return transpoortMessage;
        }

        private Type ResolveMessageType()
        {
            Type resolvedMessageType = TypeResolver.ResolveType(MessageType);

            return resolvedMessageType;
        }

        public class MongoAddress
        {
            public MongoAddress(string queue, string datacenter)
            {
                Queue = queue;
                Datacenter = datacenter;
            }

            [BsonElement("q")]
            public string Queue { get; set; }

            [BsonElement("dc")]
            public string Datacenter { get; set; }
        }
    }
}