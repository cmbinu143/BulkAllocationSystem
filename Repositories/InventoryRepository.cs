using BulkAllocation.Api.Data;
using BulkAllocation.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BulkAllocation.Api.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly ApplicationDbContext _context;

    public InventoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Inventory?> GetByProductIdAsync(int productId)
    {
        return await _context.Inventories
            .Include(x => x.Product)
            .FirstOrDefaultAsync(x => x.ProductId == productId);
    }

    public async Task<List<Inventory>> GetAllAsync()
    {
        return await _context.Inventories
            .Include(x => x.Product)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}