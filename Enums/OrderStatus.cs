namespace BulkAllocation.Api.Enums;

public enum OrderStatus
{
    Pending = 1,
    Allocated = 2,
    PartiallyAllocated = 3,
    Failed = 4,
    Cancelled = 5,
    Compensated = 6
}