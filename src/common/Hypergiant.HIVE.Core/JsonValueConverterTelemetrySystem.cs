using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hypergiant.HIVE
{
    internal class JsonValueConverterTelemetrySystem : JsonConverter<TelemetrySystem>
    {
        public override TelemetrySystem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var system = new TelemetrySystem();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return system;
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
                            case "id":
                                system.ID = val;
                                break;
                            case "mnemonic":
                                system.Mnemonic = val;
                                break;
                        }
                        break;
                    case JsonTokenType.StartArray:
                        while (reader.TokenType != JsonTokenType.EndArray)
                        {
                            reader.Read();

                            switch (propertyName)
                            {
                                case "systems":
                                    if (reader.TokenType == JsonTokenType.StartObject)
                                    {
                                        var s = (JsonValueConverterTelemetrySystem)options.GetConverter(typeof(TelemetrySystem));
                                        var sub = s.Read(ref reader, typeToConvert, options);
                                        system.Systems.Add(sub);
                                    }
                                    break;
                                case "items":
                                    if (reader.TokenType == JsonTokenType.StartObject)
                                    {
                                        var s2 = (JsonValueConverterTelemetryDataItem)options.GetConverter(typeof(TelemetryDataItem));
                                        var di = s2.Read(ref reader, typeToConvert, options);
                                        system.Items.Add(di);
                                    }
                                    break;
                            }
                        }
                        break;
                }
            }

            return system;
        }

        public override void Write(Utf8JsonWriter writer, TelemetrySystem value, JsonSerializerOptions options)
        {
            throw new NotSupportedException("Use a default Converter");
        }
    }
}