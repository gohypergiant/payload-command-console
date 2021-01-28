using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hypergiant.HIVE.HGSExecutive
{
    public class StorageProvider
    {
        public int ServicePort { get; set; }
        private HttpClient HttpClient { get; }
        private JsonSerializerOptions m_options;

        public const string ServiceProtocol = "http";
        public const string ServiceAddress = "localhost";
        public const string ServiceRoot = "/api/v1";

        public StorageProvider()
        {
            HttpClient = new HttpClient();
            m_options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true,
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task AddCommandHistory(IEnumerable<CommandHistory> history)
        {
            foreach (var h in history)
            {
                await AddCommandHistory(h);
            }
        }

        public async Task<bool> AddCommandHistory(CommandHistory history)
        {
            var path = $"{ServiceProtocol}://{ServiceAddress}:{ServicePort}{ServiceRoot}/commands/{history.CommandID}/history";
            var json = JsonSerializer.Serialize(history, m_options);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var cr = await HttpClient.PostAsync(path, content);

            if (!cr.IsSuccessStatusCode)
            {
                // TODO: logging
                // TODO: cache and forward?
                Debug.WriteLine($"{cr.StatusCode}:{cr.ReasonPhrase}");
                return false;
            }

            return true;
        }

        public async Task<bool> AddTelemetry(JsonElement telemetry)
        {
            var path = $"{ServiceProtocol}://{ServiceAddress}:{ServicePort}{ServiceRoot}/telemetry/data";
            var content = new StringContent(telemetry.ToString(), Encoding.UTF8, "application/json");
            var cr = await HttpClient.PostAsync(path, content);

            if (!cr.IsSuccessStatusCode)
            {
                // TODO: logging
                // TODO: cache and forward?
                Debug.WriteLine($"{cr.StatusCode}:{cr.ReasonPhrase}");
                return false;
            }

            return true;
        }

        public async Task<bool> AddTelemetryMeta(JsonElement telemetry)
        {
            var path = $"{ServiceProtocol}://{ServiceAddress}:{ServicePort}{ServiceRoot}/telemetry/meta";
            var content = new StringContent(telemetry.ToString(), Encoding.UTF8, "application/json");
            var cr = await HttpClient.PostAsync(path, content);

            if (!cr.IsSuccessStatusCode)
            {
                // TODO: logging
                // TODO: cache and forward?
                Debug.WriteLine($"{cr.StatusCode}:{cr.ReasonPhrase}");
                return false;
            }

            return true;
        }
    }
}