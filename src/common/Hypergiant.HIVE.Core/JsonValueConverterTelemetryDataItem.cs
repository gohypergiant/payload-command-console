using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hypergiant.HIVE
{
    public class JsonValueConverterTelemetryDataItem : JsonConverter<TelemetryDataItem>
    {
        public override TelemetryDataItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var item = new TelemetryDataItem("1", typeof(object));

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return item;
                }

                // Get the key.
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }
                string propertyName = reader.GetString();
                reader.Read();
                // get the value
                switch (reader.TokenType)
                {
                    case JsonTokenType.String:
                        var val = reader.GetString();
                        // set the property
                        // TODO: this is hard-coded ugly, but avoids the slowness of using reflection
                        // If we add more properties, re-visit this
                        switch (propertyName)
                        {
                            case "mnemonic":
                                item.Mnemonic = val;
                                break;
                            case "systemIdentifier":
                                item.SystemIdentifier = val;
                                break;
                            case "key":
                                item.Key = val;
                                break;
                        }
                        break;
                    case JsonTokenType.EndArray:
                        break;
                }
            }

            return item;
        }

        public override void Write(Utf8JsonWriter writer, TelemetryDataItem value, JsonSerializerOptions options)
        {
            throw new NotSupportedException("Use a default Converter");
        }
    }
}