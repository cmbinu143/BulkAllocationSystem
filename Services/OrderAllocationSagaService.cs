using BulkAllocation.Api.Enums;
using BulkAllocation.Api.Models;
using BulkAllocation.Api.Repositories;

namespace BulkAllocation.Api.Services;

public class OrderAllocationSagaService : IOrderAllocationSagaService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IOrderRepository _orderRepository;

    public OrderAllocationSagaService(
        IInventoryRepository inventoryRepository,
        IOrderRepository orderRepository)
    {
        _inventoryRepository = inventoryRepository;
        _orderRepository = orderRepository;
    }

    public async Task CancelOrderAsync(Order order)
    {
        // Pending order - nothing to compensate
        if (order.Status == OrderStatus.Pending)
        {
            order.SetStatus(OrderStatus.Cancelled);

            await _orderRepository.SaveChangesAsync();
            return;
        }

        // Already cancelled
        if (order.Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "Order has already been cancelled.");
        }

        // Already compensated
        if (order.Status == OrderStatus.Compensated)
        {
            throw new InvalidOperationException(
                "Order has already been compensated.");
        }

        // Failed order - no successful allocation to compensate
        if (order.Status == OrderStatus.Failed)
        {
            throw new InvalidOperationException(
                "Failed orders cannot be cancelled.");
        }

        // Compensate allocated/partially allocated order
        if (order.Status == OrderStatus.Allocated ||
            order.Status == OrderStatus.PartiallyAllocated)
        {
            foreach (var item in order.Items)
            {
                if (item.AllocatedQuantity <= 0)
                    continue;

                var inventory = await _inventoryRepository
                    .GetByProductIdAsync(item.ProductId);

                if (inventory == null)
                {
                    throw new InvalidOperationException(
                        $"Inventory not found for product {item.ProductId}.");
                }

                // Return reserved inventory
                inventory.Release(item.AllocatedQuantity);

                // Remove allocation from order item
                item.ReleaseAllocation(item.AllocatedQuantity);
            }

            // Saga compensation completed
            order.SetStatus(OrderStatus.Compensated);

            await _orderRepository.SaveChangesAsync();

            return;
        }

        throw new InvalidOperationException(
            $"Order with status '{order.Status}' cannot be cancelled.");
    }
}