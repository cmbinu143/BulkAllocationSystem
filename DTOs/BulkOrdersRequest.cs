namespace BulkAllocation.Api.DTOs;

public class BulkOrdersRequest
{
    public List<BulkOrderRequest> Orders { get; set; } = new();
}