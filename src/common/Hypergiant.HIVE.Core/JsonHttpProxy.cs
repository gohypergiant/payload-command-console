using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Hypergiant.HIVE
{
    public class JsonHttpProxy : HttpProxyBase
    {
        private JsonSerializerOptions m_options;

        public JsonHttpProxy(Uri uri)
            : base(uri)
        {
            Initialize();
        }

        public JsonHttpProxy(string uri)
            : base(uri)
        {
            Initialize();
        }

        private void Initialize()
        {
            m_options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            m_options.Converters.Add(new JsonStringEnumConverter());
        }

        override public void AddHeaders(HttpRequestMessage request)
        {
            request.Headers.Add("Authorization", "foo");
        }

        override public async Task<T> Get<T>(string path)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, path);
            AddHeaders(request);

            var response = await Client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                if (data != null)
                {
                    return JsonSerializer.Deserialize<T>(data, m_options);
                }
                Console.WriteLine($"DATA: deserialized as NULL");
            }
            return null;
        }

        override public async Task<HttpResponseMessage> Post(string path)
        {
            return await Post(path, null);
        }

        override public async Task<HttpResponseMessage> Post<T>(string path, T data)
        {
            if (!data.Equals(default(T)))
            {
                var json = (data is string) ? data as string : JsonSerializer.Serialize(data);
                return await Post(path, json);
            }

            return await Post(path, null);
        }

        public async Task<HttpResponseMessage> Post(string path, string json)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, path);
            AddHeaders(request);

            if (json != null)
            {
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }
            var response = await Client.SendAsync(request);
            return response;
        }

        override public async Task<HttpResponseMessage> Put<T>(string path, T data)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, path);
            AddHeaders(request);

            var json = (data is string) ? data as string : JsonSerializer.Serialize(data, m_options);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            return response;
        }

        public override async Task<HttpResponseMessage> Delete(string path)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, path);
            AddHeaders(request);

            var response = await Client.SendAsync(request);
            return response;
        }
    }
}
