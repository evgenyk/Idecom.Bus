using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Idecom.Bus.PubSub.MongoDB
{
    public class SagaStorageEntity
    {
        [BsonElement("_id")]
        public string Id { get; set; }

        [BsonElement("dt")]
        public string DataType { get; set; }

        [BsonElement("d")]
        public string Data { get; set; }
    }
}