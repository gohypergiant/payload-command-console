namespace Hypergiant.HIVE
{
    public class SatelliteMagementService
    {

    }

    public class ProxyStorageService : IProxyStorageService
    {
        private string m_endPoint;

        public void SetServiceEndpoint(string endpoint)
        {
            m_endPoint = endpoint;
        }
    }
}
