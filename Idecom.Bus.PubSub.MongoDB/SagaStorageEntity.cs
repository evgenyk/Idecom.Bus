using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Idecom.Bus.PubSub.MongoDB
{
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

        [BsonElement("cl")]
        public bool Closed { get; set; }

        [BsonElement("dc")]
        public BsonDateTime DateClosed { get; set; }

        [BsonElement("du")]
        public DateTime DateUpdated { get; set; }

        [BsonElement("ds")]
        public DateTime DateStarted { get; set; }
    }
}