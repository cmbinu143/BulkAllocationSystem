using BulkAllocation.Api.Enums;

namespace BulkAllocation.Api.Models;

public class AllocationResult
{
    public int Id { get; private set; }

    public int OrderId { get; private set; }

    public int ProductId { get; private set; }

    public int RequestedQuantity { get; private set; }

    public int AllocatedQuantity { get; private set; }

    public AllocationStatus Status { get; private set; }

    public string? Message { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private AllocationResult()
    {
    }

    public AllocationResult(
        int orderId,
        int productId,
        int requestedQuantity,
        int allocatedQuantity,
        AllocationStatus status,
        string? message = null)
    {
        OrderId = orderId;
        ProductId = productId;
        RequestedQuantity = requestedQuantity;
        AllocatedQuantity = allocatedQuantity;
        Status = status;
        Message = message;
        CreatedAt = DateTime.UtcNow;
    }
}