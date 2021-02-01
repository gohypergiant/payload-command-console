namespace Hypergiant.HIVE
{
    /// <summary>
    /// Contains base logic for translating data from satellite-specific format to Tethys Unified Format
    /// </summary>
    public abstract class TranslatorBase : ITranslator
    {
        public abstract TelemetryDataValue TranslateTelemetry(byte[] data);
    }
}
