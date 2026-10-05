namespace BulkAllocation.Api.Models;

public class Inventory
{
    public int Id { get; private set; }

    public int ProductId { get; private set; }

    public int AvailableQuantity { get; private set; }

    public int ReservedQuantity { get; private set; }

    public Product? Product { get; private set; }

    private Inventory()
    {
    }

    public Inventory(int productId, int availableQuantity)
    {
        if (availableQuantity < 0)
            throw new ArgumentException("Available quantity cannot be negative.");

        ProductId = productId;
        AvailableQuantity = availableQuantity;
    }

    public int Reserve(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");

        var allocated = Math.Min(quantity, AvailableQuantity);

        AvailableQuantity -= allocated;
        ReservedQuantity += allocated;

        return allocated;
    }

    public void Release(int quantity)
    {
        if (quantity <= 0)
            return;

        var releaseQuantity = Math.Min(quantity, ReservedQuantity);

        ReservedQuantity -= releaseQuantity;
        AvailableQuantity += releaseQuantity;
    }

    public void ConfirmReservation(int quantity)
    {
        if (quantity <= 0)
            return;

        var confirmQuantity = Math.Min(quantity, ReservedQuantity);

        ReservedQuantity -= confirmQuantity;
    }
}