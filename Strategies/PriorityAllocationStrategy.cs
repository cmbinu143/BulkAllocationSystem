using BulkAllocation.Api.Models;

namespace BulkAllocation.Api.Strategies;

public class PriorityAllocationStrategy : IAllocationStrategy
{
    public IEnumerable<Order> SortOrders(IEnumerable<Order> orders)
    {
        return orders
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.CreatedAt)
            .ToList();
    }
}