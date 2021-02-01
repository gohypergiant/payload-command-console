using System;
using System.Text.Json.Serialization;

namespace Hypergiant.HIVE
{
    public class TelemetryDataValue
    {
        public TelemetryDataValue()
        {
        }

        public TelemetryDataValue(object value)
            : this(value, DateTime.UtcNow)
        {
        }

        public TelemetryDataValue(object value, DateTime createTime)
        {
            Created = createTime;
            Value = value;
        }

        [JsonIgnore]
        public Guid ID { get; set; }
        public DateTime Created { get; set; }
        public object Value { get; set; }
    }
}
