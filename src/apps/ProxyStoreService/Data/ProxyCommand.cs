using Hypergiant.HIVE;
using System;
using System.ComponentModel.DataAnnotations;

namespace ProxyStoreService.Data
{
    public class ProxyTelemetryMeta
    {
        /// <summary>
        /// Indicates if the current entity has been synchronized with the HIVE back-end
        /// </summary>
        public bool IsSynchronized { get; set; }
        [Key]
        public string SatelliteID { get; set; }
        public string MetaData { get; set; }
    }

    public class ProxyTelemetryRecord
    {

    }

    public class ProxyGroundStation : GroundStation
    {
        /// <summary>
        /// Indicates if the current entity has been synchronized with the HIVE back-end
        /// </summary>
        public bool IsSynchronized { get; set; }
    }

    public class ProxyGroundStationPass : GroundStationPass
    {
        /// <summary>
        /// Indicates if the current entity has been synchronized with the HIVE back-end
        /// </summary>
        public bool IsSynchronized { get; set; }
    }

    public class ProxyGroundStationPassHistoryRecord : GroundStationPassHistoryRecord
    {
        /// <summary>
        /// Indicates if the current entity has been synchronized with the HIVE back-end
        /// </summary>
        public bool IsSynchronized { get; set; }
    }

    public class ProxyCommand : Command
    {
        /// <summary>
        /// Indicates if the current entity has been synchronized with the HIVE back-end
        /// </summary>
        public bool IsSynchronized { get; set; }
    }

    public class ProxyCommandHistoryRecord : CommandHistoryRecord, IEquatable<ProxyCommandHistoryRecord>
    {
        /// <summary>
        /// Indicates if the current entity has been synchronized with the HIVE back-end
        /// </summary>
        public bool IsSynchronized { get; set; }

        public bool Equals(ProxyCommandHistoryRecord other)
        {
            if (this.RecordID != Guid.Empty && this.RecordID == other.RecordID) return true;

            if (this.Timestamp != other.Timestamp) return false;
            if (this.Data != other.Data) return false;

            return true;
        }
    }
}
