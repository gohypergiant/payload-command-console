using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Hypergiant.HIVE
{
    public class ProxyDiscoveryResponderService : IProxyService
    {
        public const int SendPort = 42001;
        public const int ReceivePort = 42002;

        private UdpClient m_client;

        public string DiscoveryRequest { get; } = "HIVE?";

        public ProxyDiscoveryResponderService()
        {
            var ep = new IPEndPoint(IPAddress.Any, ReceivePort);
            m_client = new UdpClient(ep);
            m_client.BeginReceive(ReceiveCallback, null);
        }

        private void BroadcastReply()
        {
            var reply = "PROXY";

            foreach (var addr in Dns.GetHostEntry(Dns.GetHostName())
                .AddressList
                .Where(a => a.AddressFamily == AddressFamily.InterNetwork))
            {
                reply += $"\n{addr.ToString()}";
            }

            var replyBytes = Encoding.ASCII.GetBytes(reply);
            try
            {
                Socket sock = new Socket(AddressFamily.InterNetwork,
                                            SocketType.Dgram,
                                            ProtocolType.Udp);
                sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

                IPEndPoint iep = new IPEndPoint(IPAddress.Broadcast, SendPort);
                sock.SendTo(replyBytes, iep);
                sock.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            var e = new IPEndPoint(IPAddress.Any, 0);
            var receiveBytes = m_client.EndReceive(ar, ref e);

            var message = Encoding.ASCII.GetString(receiveBytes);
            if (message == DiscoveryRequest)
            {
                BroadcastReply();
            }

            m_client.BeginReceive(ReceiveCallback, null);
        }

    }
}
