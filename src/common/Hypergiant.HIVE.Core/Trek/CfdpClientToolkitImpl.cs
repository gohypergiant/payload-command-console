namespace Hypergiant.HIVE
{
    internal class CfdpClientToolkitImpl : ICfdpClientImpl
    {
        public CfdpClientToolkitImpl(string configFilePath)
        {
            var result = TrekCfdpToolkitInterop.InitToolkitCfdp(configFilePath);

            CheckForError(result);
        }

        public void Dispose()
        {
            TrekCfdpToolkitInterop.DSCleanUp();
        }

        private void CheckForError(TrekErrorCode errorCode)
        {
            if (errorCode == TrekErrorCode.SUCCESS) return;

            throw new TrekException(errorCode);
        }

        public void Put(string localFilePath, string remoteFilePath, long remoteEntityID, CfdpClass transferClass = CfdpClass.Class2)
        {
            var result = TrekCfdpToolkitInterop.PutComponentCFDP(localFilePath, remoteFilePath, remoteEntityID, transferClass);
            CheckForError(result);
        }

        public void EnqueuePut(string localFilePath, string remoteFilePath, long remoteEntityID, CfdpClass transferClass = CfdpClass.Class2)
        {
            var result = TrekCfdpToolkitInterop.AddPutComponentRequest(localFilePath, remoteFilePath, remoteEntityID, transferClass);

            CheckForError(result);
        }

        public void SendAllPutRequests()
        {
            var result = TrekCfdpToolkitInterop.SendAllPutRequests();

            CheckForError(result);
        }

        public void ClearAllPutRequests()
        {
            var result = TrekCfdpToolkitInterop.RemoveAllPutRequests();

            CheckForError(result);
        }
    }
}
