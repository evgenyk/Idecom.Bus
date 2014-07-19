namespace Idecom.Bus.Serializer.JsonNet.Converters
{
    using System;
    using Newtonsoft.Json;

    public class UriConverter : JsonConverter
    {
        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
                writer.WriteNull();
            else
                writer.WriteValue(value.ToString());
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Null:
                    return null;
                case JsonToken.String:
                    return new Uri((string) reader.Value, UriKind.RelativeOrAbsolute);
                default:
                    throw new Exception(
                        String.Format(
                            "Unexpected token when parsing URI. Expected String, got {0}.", reader.TokenType));
            }
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof (Uri);
        }
    }
}