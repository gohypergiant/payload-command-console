namespace Hypergiant.HIVE
{
    public enum CommandState
    {
        Unknown = -1,
        Invalid,
        AwaitingApproval,
        PendingDelivery,
        Sending,
        Delivered,
        Executing,
        ExecutionComplete,
        ArrovalDenied,
        DeliveryFailed,
        ExecutionFailed
    }
}
