using BulkAllocation.Api.Data;
using BulkAllocation.Api.Enums;
using BulkAllocation.Api.Models;
using BulkAllocation.Api.Repositories;
using BulkAllocation.Api.Strategies;

namespace BulkAllocation.Api.Services;

public class AllocationService : IAllocationService
{
    private readonly ApplicationDbContext _context;
    private readonly IOrderRepository _orderRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IAllocationStrategy _allocationStrategy;
    private readonly IRedisService _redisService;

    public AllocationService(
        ApplicationDbContext context,
        IOrderRepository orderRepository,
        IInventoryRepository inventoryRepository,
        IAllocationStrategy allocationStrategy,
        IRedisService redisService)
    {
        _context = context;
        _orderRepository = orderRepository;
        _inventoryRepository = inventoryRepository;
        _allocationStrategy = allocationStrategy;
        _redisService = redisService;
    }

    public async Task<int> RunAllocationAsync()
    {
        const string lockKey = "bulk-allocation:lock";
        var lockValue = Guid.NewGuid().ToString();

        // Acquire Redis distributed lock
        var lockAcquired = await _redisService.AcquireLockAsync(
            lockKey,
            lockValue,
            TimeSpan.FromMinutes(2));

        if (!lockAcquired)
        {
            throw new InvalidOperationException(
                "Allocation is already running. Please try again later.");
        }

        try
        {
            var orders = await _orderRepository.GetAllAsync();

            var pendingOrders = orders
                .Where(x => x.Status == OrderStatus.Pending)
                .ToList();

            if (!pendingOrders.Any())
            {
                return 0;
            }

            var sortedOrders = _allocationStrategy
                .SortOrders(pendingOrders)
                .ToList();

            var totalOrders = sortedOrders.Count;

            var batch = AllocationBatch.Create();

            _context.AllocationBatches.Add(batch);

            await _context.SaveChangesAsync();

            batch.Start();

            // Initial Redis progress
            await _redisService.SetBatchProgressAsync(
                batch.BatchId,
                totalOrders,
                0,
                "Running");

            var processedOrders = 0;

            foreach (var order in sortedOrders)
            {
                order.SetBatch(batch.Id);

                var hasAllocation = false;
                var hasPartialAllocation = false;

                foreach (var item in order.Items)
                {
                    var inventory = await _inventoryRepository
                        .GetByProductIdAsync(item.ProductId);

                    if (inventory == null)
                    {
                        _context.AllocationResults.Add(
                            new AllocationResult(
                                order.Id,
                                item.ProductId,
                                item.RequestedQuantity,
                                0,
                                AllocationStatus.Failed,
                                "Inventory not found."));

                        continue;
                    }

                    var allocatedQuantity =
                        inventory.Reserve(item.RequestedQuantity);

                    item.Allocate(allocatedQuantity);

                    if (allocatedQuantity == item.RequestedQuantity)
                    {
                        hasAllocation = true;

                        _context.AllocationResults.Add(
                            new AllocationResult(
                                order.Id,
                                item.ProductId,
                                item.RequestedQuantity,
                                allocatedQuantity,
                                AllocationStatus.Allocated,
                                "Order item fully allocated."));
                    }
                    else if (allocatedQuantity > 0)
                    {
                        hasAllocation = true;
                        hasPartialAllocation = true;

                        _context.AllocationResults.Add(
                            new AllocationResult(
                                order.Id,
                                item.ProductId,
                                item.RequestedQuantity,
                                allocatedQuantity,
                                AllocationStatus.PartiallyAllocated,
                                "Order item partially allocated."));
                    }
                    else
                    {
                        _context.AllocationResults.Add(
                            new AllocationResult(
                                order.Id,
                                item.ProductId,
                                item.RequestedQuantity,
                                0,
                                AllocationStatus.Failed,
                                "Insufficient inventory."));
                    }
                }

                if (!hasAllocation)
                {
                    order.SetStatus(OrderStatus.Failed);
                }
                else if (
                    hasPartialAllocation ||
                    order.Items.Any(x => x.PendingQuantity > 0))
                {
                    order.SetStatus(OrderStatus.PartiallyAllocated);
                }
                else
                {
                    order.SetStatus(OrderStatus.Allocated);
                }

                // Update Redis progress after each order
                processedOrders++;

                await _redisService.SetBatchProgressAsync(
                    batch.BatchId,
                    totalOrders,
                    processedOrders,
                    "Running");
            }

            batch.Complete();

            // Final Redis progress
            await _redisService.SetBatchProgressAsync(
                batch.BatchId,
                totalOrders,
                totalOrders,
                "Completed");

            await _context.SaveChangesAsync();

            return batch.Id;
        }
        finally
        {
            // Release only our own lock
            await _redisService.ReleaseLockAsync(
                lockKey,
                lockValue);
        }
    }
}