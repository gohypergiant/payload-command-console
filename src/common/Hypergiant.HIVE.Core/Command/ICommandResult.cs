using System;

namespace Hypergiant.HIVE
{
    public interface ICommandResult
    {
        Guid CommandID { get; }
        string DestinationID { get; }
        CommandState CurrentState { get; set; }
        DateTime? CreateDate { get; }
        DateTime? DeliveredDate { get; set; }
        DateTime? ExecutionStartDate { get; set; }
        DateTime? ExecutionCompleteDate { get; set; }
        byte[] Payload { get; set; }

    }
}
