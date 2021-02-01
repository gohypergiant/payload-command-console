using System;

namespace Hypergiant.HIVE
{
    public delegate void MessageIngestionHandler(byte[] message);
    public delegate Guid CommandIngestionHandler(ICommand command);

    public interface ITelemetryIngestor : IIngestor
    {
        event MessageIngestionHandler TelemetryIngested;
    }

    public interface ICommandIngestor : IIngestor
    {
        event CommandIngestionHandler CommandIngested;
    }

    public interface IPayloadIngestor : IIngestor
    {
        event MessageIngestionHandler MessageIngested;
    }

    public interface IIngestor
    {
        void Start();
        void Stop();
    }
}
