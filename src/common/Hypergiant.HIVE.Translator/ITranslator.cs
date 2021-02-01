namespace Hypergiant.HIVE
{
    public interface ITranslator
    {
        /// <summary>
        /// Translates satellite-specific message to Tethys-deserializable JSON
        /// </summary>
        /// <param name="data">Binary representation of the satellite- or ground-station-specific telemtry data</param>
        /// <returns>A JSON string that is deserializable to well-known Tethys Unified Format objects</returns>
        TelemetryDataValue TranslateTelemetry(byte[] data);
    }
}
