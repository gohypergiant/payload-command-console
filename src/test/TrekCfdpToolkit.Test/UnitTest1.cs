using Hypergiant.HIVE;
using System;
using Xunit;

namespace TrekCfdpToolkit.Test
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            using (var client = new CfdpClient("cfdp-config.txt"))
            {
                client.Put("c:\\temp\\brick.jpg", "c:\\temp\\brick_1.jpg", 100);

                client.EnqueuePut("c:\\temp\\brick.jpg", "c:\\temp\\brick_2.jpg", 100);
                client.EnqueuePut("c:\\temp\\brick.jpg", "c:\\temp\\brick_3.jpg", 100);
                client.EnqueuePut("c:\\temp\\brick.jpg", "c:\\temp\\brick_4.jpg", 100);
                Assert.Equal(3, client.PutRequests.Count);
                client.SendAllPutRequests();
                Assert.Equal(0, client.PutRequests.Count);
            }
        }
    }
}
