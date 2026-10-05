namespace BulkAllocation.Api.Enums;

public enum BatchStatus
{
    Pending = 1,
    Running = 2,
    Completed = 3,
    Failed = 4,
    Compensated = 5
}