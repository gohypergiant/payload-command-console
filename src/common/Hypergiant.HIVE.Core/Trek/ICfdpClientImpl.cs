using System;

namespace Hypergiant.HIVE
{
    internal interface ICfdpClientImpl : IDisposable
    {
        void Put(string localFilePath, string remoteFilePath, long remoteEntityID, CfdpClass transferClass = CfdpClass.Class2);
        void EnqueuePut(string localFilePath, string remoteFilePath, long remoteEntityID, CfdpClass transferClass = CfdpClass.Class2);
        void SendAllPutRequests();
        void ClearAllPutRequests();
    }
}
