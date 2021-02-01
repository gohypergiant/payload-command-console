using System;
using System.Runtime.InteropServices;

namespace Hypergiant.HIVE
{
    public class CfdpClient : IDisposable
    {
        private ICfdpClientImpl m_implementation;

        public CfdpTransferRequestCollection PutRequests { get; }

        public CfdpClient(string configFilePath)
        {
            // due to a managed loader error I can't yet figure out, we have to use CFDP on raspbian as a C++ app that we call via REST
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                m_implementation = new CfdpClientToolkitImpl(configFilePath);
            }
            else
            {
                m_implementation = new CfdpClientRestServiceImpl(configFilePath);
            }

            PutRequests = new CfdpTransferRequestCollection();
        }

        public void Dispose()
        {
            m_implementation.Dispose();
        }

        public void Put(string localFilePath, string remoteFilePath, long remoteEntityID, CfdpClass transferClass = CfdpClass.Class2)
        {
            m_implementation.Put(localFilePath, remoteFilePath, remoteEntityID, transferClass);
        }

        public void EnqueuePut(string localFilePath, string remoteFilePath, long remoteEntityID, CfdpClass transferClass = CfdpClass.Class2)
        {
            m_implementation.EnqueuePut(localFilePath, remoteFilePath, remoteEntityID, transferClass);

            PutRequests.Add(new CfdpTransferRequest(localFilePath, remoteFilePath, remoteEntityID, transferClass));
        }

        public void SendAllPutRequests()
        {
            m_implementation.SendAllPutRequests();

            PutRequests.Clear();
        }

        public void ClearAllPutRequests()
        {
            m_implementation.ClearAllPutRequests();

            PutRequests.Clear();
        }
    }
}
