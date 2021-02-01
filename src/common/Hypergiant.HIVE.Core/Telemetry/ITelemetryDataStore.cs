using System.Collections.Generic;

namespace Hypergiant.HIVE
{
    public interface ITelemetryDataStore
    {
        void UpdateTelemetrySystem(TelemetrySystem system);
        void AddDataValue(string path, TelemetryDataValue value);
        void AddDataValues(string path, IEnumerable<TelemetryDataValue> values);
        Dictionary<string, TelemetryDataValue[]> GetUnpublishedTelemetry();
        void MarkTelemetryAsPublished(IEnumerable<TelemetryDataValue> values);
        Dictionary<string, TelemetryDataValue[]> GetTelemetryValues();
        void DeleteTelemetryValues(IEnumerable<TelemetryDataValue> values);
    }
}
