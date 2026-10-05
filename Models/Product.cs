namespace BulkAllocation.Api.Models;

public class Product
{
    public int Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string SKU { get; private set; } = string.Empty;

    public decimal Price { get; private set; }

    public Inventory? Inventory { get; private set; }

    private Product()
    {
    }

    public Product(string name, string sku, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required.");

        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU is required.");

        if (price < 0)
            throw new ArgumentException("Price cannot be negative.");

        Name = name;
        SKU = sku;
        Price = price;
    }
}