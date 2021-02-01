using System;
using System.ComponentModel.DataAnnotations;

namespace Hypergiant.HIVE
{
    public class CommandHistoryRecord : CommandHistory
    {
        [Key]
        public Guid RecordID { get; set; }
        public string DestinationID { get; set; }
        public Guid GroundStationPassID { get; set; }
        /*
        public Guid? UserID { get; set; }
        public Guid? CreatedBy { get; set; }
        public string Summary { get; set; }
        */
    }
}
