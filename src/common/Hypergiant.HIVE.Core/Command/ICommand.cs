using System;

namespace Hypergiant.HIVE
{
    public interface ICommand
    {
        Guid CommandID { get; }
        Guid? ParentCommandID { get; }
        Guid? SiblingCommandID { get; }
        string Name { get; }
        CommandState CurrentState { get; }
        Guid? CreatedBy { get; }
        DateTime? CreatedOn { get; }
        string Mnemonic { get; }
        CommandPriority Priority { get; }
        bool RequiresApproval { get; }
        Guid? ApprovedBy { get; }
        DateTime? ApprovedOn { get; }
        string SourceID { get; }
        string DestinationID { get; }
        string RawPayload { get; }
        string ExecuteLocation { get; }
    }
}
