using BulkAllocation.Api.Enums;

namespace BulkAllocation.Api.DTOs;

public class BulkOrderRequest
{
    public string CustomerName { get; set; } = string.Empty;

    public OrderPriority Priority { get; set; }

    public List<BulkOrderItemRequest> Items { get; set; } = new();
}