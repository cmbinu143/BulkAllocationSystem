using BulkAllocation.Api.Models;

namespace BulkAllocation.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (context.Products.Any())
        {
            return;
        }

        var product1 = new Product(
            "Product 1",
            "PROD-001",
            100);

        var product2 = new Product(
            "Product 2",
            "PROD-002",
            200);

        context.Products.Add(product1);
        context.Products.Add(product2);

        await context.SaveChangesAsync();

        var inventory1 = new Inventory(
            product1.Id,
            5);

        var inventory2 = new Inventory(
            product2.Id,
            10);

        context.Inventories.Add(inventory1);
        context.Inventories.Add(inventory2);

        await context.SaveChangesAsync();
    }
}