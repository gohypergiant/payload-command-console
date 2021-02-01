namespace Hypergiant.HIVE
{
    public interface ITelemetryDataProvider
    {
        event TelemetryDataHandler DataAvailable;
    }
}