using BulkAllocation.Api.Enums;

namespace BulkAllocation.Api.Models;

public class AllocationBatch
{
    public int Id { get; private set; }

    public string BatchId { get; private set; } = string.Empty;

    public BatchStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? CompletedAt { get; private set; }

    public ICollection<Order> Orders { get; private set; } =
        new List<Order>();

    private AllocationBatch()
    {
    }

    public static AllocationBatch Create()
    {
        return new AllocationBatch
        {
            BatchId = Guid.NewGuid().ToString("N"),
            CreatedAt = DateTime.UtcNow,
            Status = BatchStatus.Pending
        };
    }

    public void Start()
    {
        Status = BatchStatus.Running;
    }

    public void Complete()
    {
        Status = BatchStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Fail()
    {
        Status = BatchStatus.Failed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Compensate()
    {
        Status = BatchStatus.Compensated;
        CompletedAt = DateTime.UtcNow;
    }
}