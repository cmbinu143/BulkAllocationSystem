using BulkAllocation.Api.Data;
using BulkAllocation.Api.Enums;
using BulkAllocation.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BulkAllocation.Api.Controllers;

[ApiController]
[Route("api/allocation")]
public class AllocationController : ControllerBase
{
    private readonly IAllocationService _allocationService;
    private readonly ApplicationDbContext _context;
    private readonly IRedisService _redisService;
    public AllocationController(
    IAllocationService allocationService,
    ApplicationDbContext context,
    IRedisService redisService)
    {
        _allocationService = allocationService;
        _context = context;
        _redisService = redisService;
    }
    [HttpGet("batches/{batchId}/progress")]
    public async Task<IActionResult> GetBatchProgress(string batchId)
    {
        var progress = await _redisService
            .GetBatchProgressAsync(batchId);

        if (progress == null)
        {
            return NotFound(new
            {
                message = "Batch progress not found."
            });
        }

        return Content(
            progress,
            "application/json");
    }

    [HttpPost("run")]
    public async Task<IActionResult> RunAllocation()
    {
        var batchId = await _allocationService.RunAllocationAsync();

        if (batchId == 0)
        {
            return BadRequest(new
            {
                message = "No pending orders available for allocation."
            });
        }

        var batch = await _context.AllocationBatches
            .FirstOrDefaultAsync(x => x.Id == batchId);

        var orders = await _context.Orders
            .Include(x => x.Items)
            .Where(x => x.AllocationBatchId == batchId)
            .ToListAsync();

        return Ok(new
        {
            batchId = batch?.BatchId,
            status = batch?.Status.ToString(),

            totalOrders = orders.Count,

            allocatedOrders = orders.Count(x =>
                x.Status == OrderStatus.Allocated),

            partiallyAllocatedOrders = orders.Count(x =>
                x.Status == OrderStatus.PartiallyAllocated),

            failedOrders = orders.Count(x =>
                x.Status == OrderStatus.Failed)
        });
    }

    [HttpGet("batches/{batchId}")]
    public async Task<IActionResult> GetBatch(string batchId)
    {
        var batch = await _context.AllocationBatches
            .FirstOrDefaultAsync(x => x.BatchId == batchId);

        if (batch == null)
        {
            return NotFound(new
            {
                message = "Allocation batch not found."
            });
        }

        var orders = await _context.Orders
            .Include(x => x.Items)
            .Where(x => x.AllocationBatchId == batch.Id)
            .ToListAsync();

        return Ok(new
        {
            batchId = batch.BatchId,
            status = batch.Status.ToString(),
            createdAt = batch.CreatedAt,
            completedAt = batch.CompletedAt,

            totalOrders = orders.Count,

            allocatedOrders = orders.Count(x =>
                x.Status == OrderStatus.Allocated),

            partiallyAllocatedOrders = orders.Count(x =>
                x.Status == OrderStatus.PartiallyAllocated),

            failedOrders = orders.Count(x =>
                x.Status == OrderStatus.Failed),

            orders = orders.Select(x => new
            {
                orderId = x.Id,
                customerName = x.CustomerName,
                priority = x.Priority.ToString(),
                status = x.Status.ToString(),

                items = x.Items.Select(item => new
                {
                    productId = item.ProductId,
                    requestedQuantity = item.RequestedQuantity,
                    allocatedQuantity = item.AllocatedQuantity,
                    pendingQuantity = item.PendingQuantity
                })
            })
        });
    }
}