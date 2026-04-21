namespace MyCarBE.Domain.Enums;

public enum WorkOrderStatus
{
    Received,
    Diagnosing,
    AwaitingApproval,
    InProgress,
    Completed,
    Delivered,
    Cancelled
}
