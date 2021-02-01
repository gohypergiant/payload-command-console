using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Hypergiant.HIVE.Rest
{
    public sealed class HttpRequest
    {
        private const int ReceiveBufferSize = 4096;
        private readonly byte[] m_buffer = new byte[ReceiveBufferSize];
        private readonly List<byte> m_payload = new List<byte>();
        private readonly AutoResetEvent m_parseComplete = new AutoResetEvent(false);
        private string m_content = null;

        public string Verb { get; private set; }
        public string Path { get; private set; }
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();
        public QueryString QueryString { get; private set; }
        public long ContentLength { get; private set; }

        public string Content
        {
            get
            {
                // lazy load
                if (m_content == null)
                {
                    m_content = (m_payload == null) ? string.Empty : Encoding.UTF8.GetString(m_payload.ToArray());
                }
                return m_content;
            }
        }

        public List<byte> GetContentBytes()
        {
            return m_payload;
        }

        public T GetContentAs<T>()
        {
            var opts = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<T>(Content, opts);
        }

        private HttpRequest(NetworkStream stream)
        {
            ReadMore(stream);
        }

        private void ReadMore(NetworkStream stream)
        {
            stream.BeginRead(m_buffer, 0, m_buffer.Length, ReadCallback, stream);
        }

        private void ReadCallback(IAsyncResult result)
        {
            try
            {
                var stream = (NetworkStream)result.AsyncState;
                var read = stream.EndRead(result);
                var offset = 0;

                if (m_payload.Count == 0)
                {
                    // first read
                    // first line needs to be VERB PATH VERSION
                    //   PUT http://52.14.124.164:42003/api/result/629abe11-0c0e-4cb3-aac6-08fb795c3730 HTTP/1.1
                    var lineData = m_buffer.TakeWhile(b => b != '\n').ToArray();
                    var line = Encoding.UTF8.GetString(lineData).Trim();
                    var parts = line.Split(' ');
                    if (parts.Length != 3)
                    {
                        // bad data
                        m_parseComplete.Set();
                        return;
                    }
                    if (!parts[2].StartsWith("HTTP"))
                    {
                        // bad data
                        m_parseComplete.Set();
                        return;
                    }
                    Verb = parts[0];
                    ParsePath(parts[1]);

                    offset += lineData.Length + 1;
                    // look for a Content-Length
                    //   Content-Length: 122256
                    while (true)
                    {
                        lineData = m_buffer.Skip(offset).TakeWhile(b => b != '\n').ToArray();
                        offset += lineData.Length + 1;
                        line = Encoding.UTF8.GetString(lineData).Trim();

                        if (line == string.Empty)
                        {
                            break;
                        }

                        var idx = line.IndexOf(':');
                        Headers.Add(line.Substring(0, idx), line.Substring(0, idx) + 1);

                        if (line.StartsWith("Content-Length"))
                        {
                            idx = line.LastIndexOf(' ');
                            if (long.TryParse(line.Substring(idx), out long cl))
                            {
                                ContentLength = cl;
                            }
                        }
                    }
                }

                // if we still have data, it's all content.  Pull until we have Content-Length of data
                if (read > offset)
                {
                    m_payload.AddRange(m_buffer.Skip(offset).Take(read - offset));
                }

                if (m_payload.Count < ContentLength)
                {
                    ReadMore(stream);
                }
                else
                {
                    m_parseComplete.Set();
                }
            }
            catch
            {
                return;
            }
        }

        private void ParsePath(string path)
        {
            var segments = path.Split('?');
            if (segments.Length > 1)
            {
                Path = segments[0];
                QueryString = new QueryString(segments[1]);
            }
            else
            {
                Path = path;
                QueryString = new QueryString(string.Empty);
            }
        }

        public static async Task<HttpRequest> From(TcpClient client)
        {
            var r = new HttpRequest(client.GetStream());
            await Task.Run(() => r.m_parseComplete.WaitOne(TimeSpan.FromSeconds(10)));
            return r;
        }
    }
}
