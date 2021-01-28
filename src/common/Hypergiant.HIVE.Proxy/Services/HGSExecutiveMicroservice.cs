using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hypergiant.HIVE
{
    public class HGSExecutiveMicroservice : MicroserviceBase
    {
        private string ServiceRootUri { get; set; }
        public int ServicePort { get; set; }

        public HGSExecutiveMicroservice(int servicePort = 5002, bool useDocker = false)
            : base(useDocker,
                  @".\Hypergiant.HIVE.HGSExecutive.exe",
                  "dockername",
                  "HGSExecutive",
                  "HGSExecutiveShutdown"
                  )
        {
            ServicePort = servicePort;
            ServiceRootUri = $"http://localhost:{ServicePort}/api/v1/";
        }

        public async Task<bool> DeliverCommand(ICommandEnvelope command)
        {
            var path = $"{ServiceRootUri}commands";
            var json = JsonSerializer.Serialize(command, SerializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var cr = await HttpClient.PostAsync(path, content);
            if (!cr.IsSuccessStatusCode)
            {
                Debug.WriteLine($"{cr.StatusCode}:{cr.ReasonPhrase}");
                throw new Exception(cr.ReasonPhrase);
            }

            return true;
        }

        public async Task<IEnumerable<CommandHistory>> GetCommandHistory(Guid commandID)
        {
            var path = $"{ServiceRootUri}commands/{commandID}/history";
            var cr = await HttpClient.GetAsync(path);
            if (!cr.IsSuccessStatusCode)
            {
                // TODO: log/handle
                Debug.WriteLine($"{cr.StatusCode}:{cr.ReasonPhrase}");
                return null;
            }

            var json = await cr.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            var result = JsonSerializer.Deserialize<CommandHistory[]>(json, SerializerOptions);
            return result;
        }

        public async Task<ExecutiveConfiguration> GetExecutiveConfiguration()
        {
            var path = $"{ServiceRootUri}configuration";
            var cr = await HttpClient.GetAsync(path);
            if (!cr.IsSuccessStatusCode)
            {
                // TODO: log/handle
                Debug.WriteLine($"{cr.StatusCode}:{cr.ReasonPhrase}");
                return null;
            }

            var json = await cr.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            var result = JsonSerializer.Deserialize<ExecutiveConfiguration>(json, SerializerOptions);
            return result;
        }

        public async Task<bool> UpdateExecutiveConfiguration(ExecutiveConfiguration configuration)
        {
            var path = $"{ServiceRootUri}configuration";
            var json = JsonSerializer.Serialize(configuration, SerializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var cr = await HttpClient.PutAsync(path, content);
            if (!cr.IsSuccessStatusCode)
            {
                // TODO: log/handle
                Debug.WriteLine($"{cr.StatusCode}:{cr.ReasonPhrase}");
                return false;
            }

            return true;
        }

    }
}
