using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hypergiant.HIVE
{
    public class ProxyStoreMicroservice : MicroserviceBase
    {
        private string ServiceRootUri { get; set; }
        public int ServicePort { get; set; }

        public ProxyStoreMicroservice(int servicePort = 5001, bool useDocker = false)
            : base(useDocker,
                  @".\ProxyStoreService.exe",
                  "dockername",
                  "ProxyStoreService",
                  "ProxyStoreServiceShutdown"
                  )
        {
            ServicePort = servicePort;
            ServiceRootUri = $"http://localhost:{ServicePort}/api/v1/";
        }


        public async Task<GroundStation[]> GetGroundStations()
        {
            try
            {
                var path = $"{ServiceRootUri}groundstations";
                var gsr = await HttpClient.GetAsync(path);
                if (!gsr.IsSuccessStatusCode)
                {
                    // TODO: logging
                    Debug.WriteLine($"{gsr.StatusCode}:{gsr.ReasonPhrase}");
                    return null;
                }

                var json = await gsr.Content.ReadAsStringAsync();
                var groundStations = JsonSerializer.Deserialize<GroundStation[]>(json, SerializerOptions);
                return groundStations;
            }
            catch (Exception ex)
            {
                // TODO: logging
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<GroundStationPass[]> GetPasses()
        {
            try
            {
                var path = $"{ServiceRootUri}passes";
                var pr = await HttpClient.GetAsync(path);
                if (!pr.IsSuccessStatusCode)
                {
                    // TODO: logging
                    Debug.WriteLine($"{pr.StatusCode}:{pr.ReasonPhrase}");
                    return null;
                }
                var json = await pr.Content.ReadAsStringAsync();
                var passes = JsonSerializer.Deserialize<GroundStationPass[]>(json, SerializerOptions);
                return passes;
            }
            catch (Exception ex)
            {
                // TODO: logging
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<GroundStationPassHistoryRecord[]> GetPassHistory(Guid passID)
        {
            try
            {
                var path = $"{ServiceRootUri}history/passes/{passID}";
                var psr = await HttpClient.GetAsync(path);

                if (!psr.IsSuccessStatusCode)
                {
                    // TODO: logging
                    Debug.WriteLine($"{psr.StatusCode}:{psr.ReasonPhrase}");
                    return null;
                }
                var json = await psr.Content.ReadAsStringAsync();
                var history = JsonSerializer.Deserialize<GroundStationPassHistoryRecord[]>(json, SerializerOptions);
                return history;
            }
            catch (Exception ex)
            {
                // TODO: logging
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<Command[]> GetCommandsForPass(Guid passID)
        {
            try
            {
                var path = $"{ServiceRootUri}commands?passid={passID}";
                var cr = await HttpClient.GetAsync(path);

                if (!cr.IsSuccessStatusCode)
                {
                    // TODO: logging
                    Debug.WriteLine($"{cr.StatusCode}:{cr.ReasonPhrase}");
                    return null;
                }
                var json = await cr.Content.ReadAsStringAsync();
                var commands = JsonSerializer.Deserialize<Command[]>(json, SerializerOptions);
                return commands;
            }
            catch (Exception ex)
            {
                // TODO: logging
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<bool> InsertCommand(Command command)
        {
            var path = $"{ServiceRootUri}commands";
            var json = JsonSerializer.Serialize(command);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var cr = await HttpClient.PostAsync(path, content);

            if (!cr.IsSuccessStatusCode)
            {
                // TODO: logging
                Debug.WriteLine($"{cr.StatusCode}:{cr.ReasonPhrase}");
                return false;
            }

            return true;
        }

        public async Task<bool> InsertCommandHistory(CommandHistory history)
        {
            var path = $"{ServiceRootUri}commands/{history.CommandID}/history";
            var json = JsonSerializer.Serialize(history);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var cr = await HttpClient.PostAsync(path, content);

            if (!cr.IsSuccessStatusCode)
            {
                // TODO: logging
                Debug.WriteLine($"{cr.StatusCode}:{cr.ReasonPhrase}");
                return false;
            }

            return true;
        }

        public async Task<bool> UpdatePassAos(Guid passID, DateTime timestampUtc)
        {
            var path = $"{ServiceRootUri}passes/{passID}";
            var payload = new
            {
                GroundStationPassID = passID,
                ActualAos = timestampUtc
            };

            var result = await HttpClient.PutAsync(path, new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));
            if (!result.IsSuccessStatusCode)
            {
                // TODO: logging
                Debug.WriteLine($"{result.StatusCode}:{result.ReasonPhrase}");
                return false;
            }

            return true;
        }

        public async Task<bool> UpdatePassLos(Guid passID, DateTime timestampUtc)
        {
            var path = $"{ServiceRootUri}passes/{passID}";
            var payload = new
            {
                GroundStationPassID = passID,
                ActualLos = timestampUtc
            };

            var result = await HttpClient.PutAsync(path, new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));
            if (!result.IsSuccessStatusCode)
            {
                // TODO: logging
                Debug.WriteLine($"{result.StatusCode}:{result.ReasonPhrase}");
                return false;
            }

            return true;
        }

        public async Task<bool> InsertPassHistory(GroundStationPassHistoryRecord record)
        {
            var path = $"{ServiceRootUri}passes";
            var result = await HttpClient.PostAsync(path, new StringContent(JsonSerializer.Serialize(record), Encoding.UTF8, "application/json"));
            if (!result.IsSuccessStatusCode)
            {
                // TODO: logging
                Debug.WriteLine($"{result.StatusCode}:{result.ReasonPhrase}");
                return false;
            }

            return true;
        }

        public async Task<CommandHistory[]> GetCommandHistory(Guid commandID)
        {
            var path = $"{ServiceRootUri}commands/{commandID}/history";
            var cr = await HttpClient.GetAsync(path);

            if (!cr.IsSuccessStatusCode)
            {
                // TODO: logging
                Debug.WriteLine($"{cr.StatusCode}:{cr.ReasonPhrase}");
                return null;
            }
            var json = await cr.Content.ReadAsStringAsync();
            var history = JsonSerializer.Deserialize<CommandHistory[]>(json, SerializerOptions);
            return history;

        }
    }
}
