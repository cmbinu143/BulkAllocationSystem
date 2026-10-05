using BulkAllocation.Api.Models;

namespace BulkAllocation.Api.Repositories;

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync();

    Task<Product?> GetByIdAsync(int id);

    Task<Product?> GetBySkuAsync(string sku);

    Task AddAsync(Product product);

    Task SaveChangesAsync();
}