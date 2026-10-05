using BulkAllocation.Api.Models;

namespace BulkAllocation.Api.Strategies;

public interface IAllocationStrategy
{
    IEnumerable<Order> SortOrders(IEnumerable<Order> orders);
}