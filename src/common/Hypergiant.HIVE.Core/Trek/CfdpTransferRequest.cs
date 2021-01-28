namespace Hypergiant.HIVE
{
    public class CfdpTransferRequest
    {
        public string LocalFilePath { get; }
        public string RemoteFilePath { get; }
        public long RemoteEntityID { get; }
        public CfdpClass TransferClass { get; }

        internal CfdpTransferRequest(string localFilePath, string remoteFilePath, long remoteEntityID, CfdpClass transferClass = CfdpClass.Class2)
        {
            LocalFilePath = localFilePath;
            RemoteEntityID = remoteEntityID;
            RemoteFilePath = remoteFilePath;
            TransferClass = transferClass;
        }
    }
}
