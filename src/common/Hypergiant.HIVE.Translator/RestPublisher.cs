using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Hypergiant.HIVE
{
    /// <summary>
    /// Responsible for publishing data to the Tethys back-end via REST
    /// </summary>
    public class RestPublisher : ITelemetryPublisher
    {
        private HttpClient m_client;

        public string EndpointUrl { get; }
        public TimeSpan Timespan { get; }

        public RestPublisher(string endpointUrl)
        {
            EndpointUrl = endpointUrl;

            // TODO: make configurable
            m_client = new HttpClient();
            m_client.Timeout = TimeSpan.FromSeconds(10);

            // TODO: add any default headers (auth, API keys, etc)
        }

        private string GetDestinationPath()
        {
            // TODO: this needs definition/expansion
            return EndpointUrl;
        }

        public async void QueueTelemetry(string path, TelemetryDataValue value)
        {
            try
            {
                var p = GetDestinationPath();
                var content = new StringContent(value.ToJson());
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                content.Headers.Add("Payload-Version", "v3");
                var response = await m_client.PostAsync(p, content);

                // TODO: define a standard JSON response object for failures
                if (!response.IsSuccessStatusCode)
                {
                    switch (response.StatusCode)
                    {
                        // TODO: expand this
                        default:
                            throw new Exception("Publish failed: " + response.StatusCode.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO: implement logging
                Console.WriteLine(ex.Message);
            }
        }
    }
}
