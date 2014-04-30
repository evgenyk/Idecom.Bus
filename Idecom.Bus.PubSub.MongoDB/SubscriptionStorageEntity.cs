namespace Idecom.Bus.PubSub.MongoDB
{
    using global::MongoDB.Bson;

    public class SubscriptionStorageEntity
    {
        [global::MongoDB.Bson.Serialization.Attributes.BsonElementAttribute("_id")]
        public BsonObjectId Id { get; set; }

        [global::MongoDB.Bson.Serialization.Attributes.BsonElementAttribute("mt")]
        public string MessageType { get; set; }

        [global::MongoDB.Bson.Serialization.Attributes.BsonElementAttribute("se")]
        public string SubscriberAddress { get; set; }
    }
}