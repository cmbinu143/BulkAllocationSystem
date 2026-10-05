using BulkAllocation.Api.Models;

namespace BulkAllocation.Api.Repositories;

public interface IInventoryRepository
{
    Task<Inventory?> GetByProductIdAsync(int productId);

    Task<List<Inventory>> GetAllAsync();

    Task SaveChangesAsync();
}