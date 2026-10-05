using BulkAllocation.Api.Enums;

namespace BulkAllocation.Api.Models;

public class Order
{
    public int Id { get; private set; }

    public string CustomerName { get; private set; } = string.Empty;

    public OrderPriority Priority { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public OrderStatus Status { get; private set; }

    public int? AllocationBatchId { get; private set; }

    public AllocationBatch? AllocationBatch { get; private set; }

    public ICollection<OrderItem> Items { get; private set; } = new List<OrderItem>();

    private Order()
    {
    }

    public Order(
        string customerName,
        OrderPriority priority)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Customer name is required.");

        CustomerName = customerName;
        Priority = priority;
        CreatedAt = DateTime.UtcNow;
        Status = OrderStatus.Pending;
    }

    public void AddItem(OrderItem item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        Items.Add(item);
    }

    public void SetStatus(OrderStatus status)
    {
        Status = status;
    }

    public void SetBatch(int batchId)
    {
        AllocationBatchId = batchId;
    }
}