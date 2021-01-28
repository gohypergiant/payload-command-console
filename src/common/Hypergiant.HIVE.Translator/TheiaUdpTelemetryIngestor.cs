using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Hypergiant.HIVE
{
    /// <summary>
    /// Contains logic specific to ingesting UDP data from the Hypergiant Theia Table Sat
    /// </summary>
    public class TheiaUdpTelemetryIngestor : UdpTelemetryIngestor
    {
        public override async Task MessageReceived(byte[] message)
        {
            Console.WriteLine(Encoding.UTF8.GetString(message));
            await Task.CompletedTask;
        }
    }
}
