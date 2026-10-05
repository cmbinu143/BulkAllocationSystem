namespace BulkAllocation.Api.Models;

public class OrderItem
{
    public int Id { get; private set; }

    public int OrderId { get; private set; }

    public int ProductId { get; private set; }

    public int RequestedQuantity { get; private set; }

    public int AllocatedQuantity { get; private set; }

    public int PendingQuantity =>
        RequestedQuantity - AllocatedQuantity;

    public Order? Order { get; private set; }

    public Product? Product { get; private set; }

    private OrderItem()
    {
    }

    public OrderItem(int productId, int requestedQuantity)
    {
        if (requestedQuantity <= 0)
            throw new ArgumentException(
                "Requested quantity must be greater than zero.");

        ProductId = productId;
        RequestedQuantity = requestedQuantity;
    }

    public void Allocate(int quantity)
    {
        if (quantity <= 0)
            return;

        var remaining = RequestedQuantity - AllocatedQuantity;

        var allocation = Math.Min(quantity, remaining);

        AllocatedQuantity += allocation;
    }
    public void ReleaseAllocation(int quantity)
    {
        if (quantity <= 0) return;

        AllocatedQuantity = Math.Max(
            0,
            AllocatedQuantity - quantity);
    }
}