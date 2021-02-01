namespace Hypergiant.HIVE
{
    public class ExecutiveConfiguration
    {
        public string ExecutiveType { get; set; }
        public string SatelliteAddress { get; set; }
        public string DestinationUplinkFolder { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int StorageServicePort { get; set; } = 5001;
        public string CfdpConfigFile { get; set; }
        public int SatelliteCfdpEntityID { get; set; }
    }
}
