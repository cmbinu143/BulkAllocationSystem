using BulkAllocation.Api.Models;

namespace BulkAllocation.Api.Repositories;

public interface IOrderRepository
{
    Task<List<Order>> GetAllAsync();

    Task<Order?> GetByIdAsync(int id);

    Task AddAsync(Order order);

    Task SaveChangesAsync();
}