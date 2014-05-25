using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Idecom.Bus.PubSub.MongoDB
{
    public class SubscriptionStorageEntity
    {
        [BsonElement("_id")]
        public BsonObjectId Id { get; set; }

        [BsonElement("mt")]
        public string MessageType { get; set; }

        [BsonElement("se")]
        public string SubscriberAddress { get; set; }

        [BsonElement("sb")]
        public bool Subscribed { get; set; }
    }
}