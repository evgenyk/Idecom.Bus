namespace Idecom.Bus.PubSub.MongoDB
{
    using System;
    using global::MongoDB.Bson.Serialization.Attributes;

    public class SagaStorageEntity
    {
        public SagaStorageEntity(string id, object data)
        {
            Id = id;
            Data = data;
        }

        [BsonElement("_id")]
        public string Id { get; set; }

        [BsonElement("d")]
        public object Data { get; set; }

        [BsonElement("oe")]
        public string OwnerEndpoint { get; set; }

        [BsonElement("du")]
        public DateTime DateUpdated { get; set; }

        [BsonElement("ds")]
        public DateTime DateStarted { get; set; }
    }
}