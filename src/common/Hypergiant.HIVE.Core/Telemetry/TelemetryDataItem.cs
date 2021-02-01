using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Hypergiant.HIVE
{
    public class TelemetryDataItem
    {
        private string m_path;

        public TelemetryDataItem()
        {
        }

        public TelemetryDataItem(
            string systemIdentifier,
            Type valueType,
            string units = null)
        {
            SystemIdentifier = systemIdentifier;
            ValueType = valueType.AssemblyQualifiedName;
            Units = units;
        }

        public TelemetryDataItem(
            string systemIdentifier,
            string valueType,
            string units = null)
        {
            SystemIdentifier = systemIdentifier;
            ValueType = valueType.ToString();
            Units = units;
        }

        [JsonIgnore]
        public TelemetrySystem Parent { get; internal set; }
        public string SystemIdentifier { get; set; }
        public string Mnemonic { get; set; }
        public string Key { get; set; }
        public string Units { get; set; }
        public string ValueType { get; set; }

        public string Path
        {
            set => m_path = value;
            get
            {
                if (m_path != null)
                {
                    return m_path;
                }

                if (Parent == null) return null;

                return $"{Parent.Path}/{SystemIdentifier}";
            }
        }
    }
}
