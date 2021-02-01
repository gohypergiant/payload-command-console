using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hypergiant.HIVE
{
    public class TelemetrySystemToHiveConverter : JsonConverter<TelemetrySystem>
    {
        public override TelemetrySystem Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            return null;
        }

        private string DotnetTypeToHiveTypeName(Type type)
        {
            if (type.IsGenericType)
            {
                return DotnetTypeToHiveTypeName(type.GenericTypeArguments[0]);
            }
            if (type.IsEnum)
            {
                return "enum";
            }
            switch (type.Name.ToLower())
            {
                case "byte":
                case "short":
                case "uint16":
                case "int":
                case "long":
                    return "long";
                case "float":
                case "decimal":
                case "double":
                    return "double";
                case "datetime":
                case "timespan":
                    return "datetime";
                case "string":
                    return "string";
                case "boolean":
                    return "bool";
                default:
                    return $"[unsupported]{type.Name.ToLower()}";
            }
        }

        private void Write(
            Utf8JsonWriter writer,
            TelemetryDataItem item,
            JsonSerializerOptions _)
        {
            var t = Type.GetType(item.ValueType);

            var typeName = DotnetTypeToHiveTypeName(t);
            switch (typeName)
            {
                case "long":
                case "double":
                case "datetime":
                case "string":
                case "bool":
                    writer.WriteStartObject();

                    writer.WriteString("systemIdentifier", $"{item.SystemIdentifier}");
                    if (!string.IsNullOrEmpty(item.Units))
                    {
                        writer.WriteString("units", $"{item.Units}");
                    }
                    writer.WriteString("path", $"{item.Path}");
                    writer.WriteString("valueType", typeName);

                    writer.WriteEndObject();
                    break;
                default:
                    var props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    if (props.Length > 0)
                    {
                        foreach (var p in props)
                        {
                            if (p.CanWrite && p.CanWrite)
                            {
                                writer.WriteStartObject();
                                writer.WriteString("systemIdentifier", $"{item.SystemIdentifier}.{p.Name}");
                                if (!string.IsNullOrEmpty(item.Units))
                                {
                                    writer.WriteString("units", $"{item.Units}");
                                }
                                writer.WriteString("path", $"{item.Path}/{p.Name.ToLower()}");
                                writer.WriteString("valueType", DotnetTypeToHiveTypeName(p.PropertyType));
                                writer.WriteEndObject();
                            }
                        }
                    }
                    break;
            }
        }

        public override void Write(
            Utf8JsonWriter writer,
            TelemetrySystem system,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            if (!string.IsNullOrEmpty(system.ID))
            {
                writer.WriteString("id", system.ID);
            }
            if (!string.IsNullOrEmpty(system.Path))
            {
                writer.WriteString("path", system.Path);
            }
            if (!string.IsNullOrEmpty(system.Mnemonic))
            {
                writer.WriteString("mnemonic", system.Mnemonic);
            }

            writer.WriteStartArray("items");
            foreach (var item in system.Items)
            {
                Write(writer, item, options);
            }
            writer.WriteEndArray(); // items

            if (system.Systems != null && system.Systems.Count > 0)
            {
                writer.WriteStartArray("systems");
                foreach (var sys in system.Systems)
                {
                    Write(writer, sys, options);
                }
                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }
    }
}