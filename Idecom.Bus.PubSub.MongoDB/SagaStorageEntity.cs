using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Idecom.Bus.PubSub.MongoDB
{
    public class SagaStorageEntity
    {
        [BsonElement("_id")]
        public string Id { get; set; }

        [BsonElement("d")]
        public object Data { get; set; }
    }
}