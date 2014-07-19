namespace Idecom.Bus.Serializer.JsonNet
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Serialization.Formatters;
    using Converters;
    using Interfaces;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class JsonNetMessageSerializer : IMessageSerializer
    {
        readonly IInstanceCreator _instanceCreator;
        readonly JsonSerializerSettings _jsonSerializerSettings;

        public JsonNetMessageSerializer(IInstanceCreator instanceCreator)
        {
            _instanceCreator = instanceCreator;
            _jsonSerializerSettings = new JsonSerializerSettings
                                      {
                                          TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                                          TypeNameHandling = TypeNameHandling.Auto,
                                          Converters =
                                          {
                                              new IsoDateTimeConverter {DateTimeStyles = DateTimeStyles.RoundtripKind},
                                              new StringEnumConverter(),
                                              new TimeSpanConverter(),
                                              new UriConverter()
                                          },
                                          MissingMemberHandling = MissingMemberHandling.Ignore,
                                      };
        }

        public string Serialize(object message)
        {
            var serializer = JsonSerializer.Create(_jsonSerializerSettings);
            serializer.Binder = new InterfaceSupportingBinder(_instanceCreator);
            var stringWriter = new StringWriter();
            serializer.Serialize(stringWriter, message);
            var serialized = stringWriter.ToString();
            return serialized;
        }

        public object DeSerialize(string message, Type type)
        {
            var serializer = JsonSerializer.Create(_jsonSerializerSettings);
            var deserialized = serializer.Deserialize(new StringReader(message), type);
            return deserialized;
        }
    }
}