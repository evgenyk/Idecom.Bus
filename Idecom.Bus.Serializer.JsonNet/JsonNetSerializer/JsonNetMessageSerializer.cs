using System;
using System.Globalization;
using System.Runtime.Serialization.Formatters;
using Idecom.Bus.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Idecom.Bus.Serializer.JsonNet.JsonNetSerializer
{
    public class JsonNetMessageSerializer : IMessageSerializer
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public JsonNetMessageSerializer()
        {
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = {new IsoDateTimeConverter {DateTimeStyles = DateTimeStyles.RoundtripKind}},
                MissingMemberHandling = MissingMemberHandling.Ignore,
            };
        }

        public string Serialize(object message)
        {
            string serialized = JsonConvert.SerializeObject(message, _jsonSerializerSettings);
            return serialized;
        }

        public object DeSerialize(string message, Type type)
        {
            object deserialized = JsonConvert.DeserializeObject(message, type);
            return deserialized;
        }
    }
}