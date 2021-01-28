using System.Text;
using System.Text.Json;

namespace Hypergiant.HIVE
{
    /// <summary>
    /// Contains logic specific to translating data from the Hypergiant Theia Table Sat
    /// </summary>
    public class TheiaTranslator : TranslatorBase
    {
        private JsonSerializerOptions m_options;

        public TheiaTranslator()
        {
            m_options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public override TelemetryDataValue TranslateTelemetry(byte[] data)
        {
            var s = Encoding.UTF8.GetString(data);
            return JsonSerializer.Deserialize<TelemetryDataValue>(s, m_options);
        }
    }
}
