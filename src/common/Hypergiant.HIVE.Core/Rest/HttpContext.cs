using System.Net.Sockets;

namespace Hypergiant.HIVE.Rest
{
    public class HttpContext
    {
        public TcpClient Client { get; }
        public HttpRequest Request { get; }

        internal HttpContext(TcpClient client, HttpRequest request)
        {
            Client = client;
            Request = request;
        }
    }
}
