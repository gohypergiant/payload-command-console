using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Hypergiant.HIVE
{
    public abstract class HttpProxyBase
    {
        protected HttpClient Client { get; }
        public string Address { get; }

        protected HttpProxyBase()
        {
            Client = new HttpClient();
        }

        protected HttpProxyBase(Uri uri)
            : this()
        {
            Client.BaseAddress = uri;
        }

        protected HttpProxyBase(string uri)
            : this()
        {
            // quick validation
            if (!uri.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                uri = $"http://{uri}";
            }

            Client.BaseAddress = new Uri(uri);
            Address = Client.BaseAddress.ToString();
        }

        public abstract Task<T> Get<T>(string path)
            where T : class;

        public abstract Task<HttpResponseMessage> Post(string path);
        public abstract Task<HttpResponseMessage> Post<T>(string path, T data);

        public abstract Task<HttpResponseMessage> Put<T>(string path, T data);

        public abstract Task<HttpResponseMessage> Delete(string path);

        public virtual void AddHeaders(HttpRequestMessage request) { }

        protected async Task<Stream> GetContentStream(string path)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, path);
            AddHeaders(request);

            var response = await Client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStreamAsync();
            }
            return null;
        }

        protected async Task<byte[]> GetContentBytes(string path)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, path);
            AddHeaders(request);

            var response = await Client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
            return null;
        }
    }
}
