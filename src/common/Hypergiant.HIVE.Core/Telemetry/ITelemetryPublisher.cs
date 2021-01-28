using System.Threading.Tasks;

namespace Hypergiant.HIVE
{
    public interface ITelemetryPublisher
    {
        void QueueTelemetry(string path, TelemetryDataValue value);
    }
}
