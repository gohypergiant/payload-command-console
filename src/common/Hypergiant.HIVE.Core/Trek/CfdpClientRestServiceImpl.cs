using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Hypergiant.HIVE
{
    internal class CfdpClientRestServiceImpl : ICfdpClientImpl
    {
        private HttpClient Client { get; }
        private string ApiAddress { get; }
        private int ApiPort { get; }
        private string ConfigFilePath { get; }

        public CfdpClientRestServiceImpl(string configFilePath, string apiAddress = "localhost", int apiPort = 5000)
        {
            Console.WriteLine("Creating CFDP Rest Server...");

            ConfigFilePath = configFilePath;
            Client = new HttpClient();
            ApiAddress = apiAddress;
            ApiPort = apiPort;

            Task.Run(async () =>
            {
                if (!await IsCfdpServerRunning())
                {
                    Console.WriteLine("CFDP Daemon is not running.");
                    await StartCfdpRestServer();
                }
                else
                {
                    Console.WriteLine($"CFDP Daemon found at http://{ApiAddress}:{ApiPort}/api");
                }
            });

        }

        private async Task<bool> IsCfdpServerRunning()
        {
            //check for server exitence
            try
            {
                var result = await Client.GetAsync($"http://{ApiAddress}:{ApiPort}/api");

                // really any content probably indicates it's up
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception trying to reach CFDP Rest Server: {ex.GetType().Name}");
                return false;
            }
        }

        private async Task StartCfdpRestServer()
        {
            Console.WriteLine("Starting CFDP Daemon...");

            await Task.CompletedTask;
        }

        public void ClearAllPutRequests()
        {
        }

        public void Dispose()
        {
        }

        public void EnqueuePut(string localFilePath, string remoteFilePath, long remoteEntityID, CfdpClass transferClass = CfdpClass.Class2)
        {
        }

        public void Put(string localFilePath, string remoteFilePath, long remoteEntityID, CfdpClass transferClass = CfdpClass.Class2)
        {
        }

        public void SendAllPutRequests()
        {
        }
    }
}
