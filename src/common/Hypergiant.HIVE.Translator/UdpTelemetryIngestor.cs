using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Hypergiant.HIVE
{
    /// <summary>
    /// Contains base logic for ingesting satellite data over UDP
    /// </summary>
    public abstract class UdpTelemetryIngestor : ITelemetryIngestor
    {
        public const int DefaultUdpPort = 4242;

        public event MessageIngestionHandler TelemetryIngested;

        private UdpClient m_client;
        private CancellationTokenSource m_cancelSource;

        public int Port { get; }

        public abstract Task MessageReceived(byte[] message);

        public UdpTelemetryIngestor(int port = DefaultUdpPort)
        {
            Port = port;
            m_client = new UdpClient();
        }

        public void Start()
        {
            m_cancelSource = new CancellationTokenSource();
            m_client.EnableBroadcast = true;
            m_client.Client.Bind(new IPEndPoint(IPAddress.Any, Port));
            ReceiveLoop();
        }

        protected void RaiseMessageIngested(byte[] data)
        {
            TelemetryIngested?.Invoke(data);
        }

        private void ReceiveLoop()
        {
            Task.Run(() =>
            {
                var sender = new IPEndPoint(0, 0);
                while (true)
                {
                    var recvBuffer = m_client.Receive(ref sender);

                    _ = MessageReceived(recvBuffer);

                    TelemetryIngested(recvBuffer);
                }
            }, m_cancelSource.Token);
        }

        public void Stop()
        {
            m_cancelSource.Cancel();
        }

    }
}
