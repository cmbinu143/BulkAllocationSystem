namespace BulkAllocation.Api.DTOs;

public class BulkOrderItemRequest
{
    public int ProductId { get; set; }

    public int Quantity { get; set; }
}