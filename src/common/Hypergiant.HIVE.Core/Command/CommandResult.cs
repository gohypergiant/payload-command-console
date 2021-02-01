using System;

namespace Hypergiant.HIVE
{
    public class CommandResult : ICommandResult
    {
        public Guid CommandID { get; set; }
        public string DestinationID { get; set; }
        public CommandState CurrentState { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime? ExecutionStartDate { get; set; }
        public DateTime? ExecutionCompleteDate { get; set; }
        public byte[] Payload { get; set; }

        public CommandResult()
        {
        }

        public CommandResult(Guid commandID, string destinationID)
        {
            CommandID = commandID;
            DestinationID = destinationID;
        }
    }
}
