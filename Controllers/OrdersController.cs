using BulkAllocation.Api.DTOs;
using BulkAllocation.Api.Enums;
using BulkAllocation.Api.Models;
using BulkAllocation.Api.Repositories;
using BulkAllocation.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BulkAllocation.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOrderAllocationSagaService _sagaService;

    public OrdersController(
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    IOrderAllocationSagaService sagaService)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _sagaService = sagaService;
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> CreateBulk(
        [FromBody] BulkOrdersRequest request)
    {
        if (request.Orders == null || request.Orders.Count == 0)
        {
            return BadRequest("At least one order is required.");
        }

        var createdOrders = new List<Order>();

        foreach (var orderRequest in request.Orders)
        {
            if (string.IsNullOrWhiteSpace(orderRequest.CustomerName))
            {
                return BadRequest("Customer name is required.");
            }

            if (orderRequest.Items == null ||
                orderRequest.Items.Count == 0)
            {
                return BadRequest(
                    $"At least one item is required for {orderRequest.CustomerName}.");
            }

            var order = new Order(
                orderRequest.CustomerName,
                orderRequest.Priority);

            foreach (var itemRequest in orderRequest.Items)
            {
                if (itemRequest.Quantity <= 0)
                {
                    return BadRequest(
                        "Quantity must be greater than zero.");
                }

                var product = await _productRepository
                    .GetByIdAsync(itemRequest.ProductId);

                if (product == null)
                {
                    return BadRequest(
                        $"Product {itemRequest.ProductId} does not exist.");
                }

                order.AddItem(
                    new OrderItem(
                        itemRequest.ProductId,
                        itemRequest.Quantity));
            }

            await _orderRepository.AddAsync(order);

            createdOrders.Add(order);
        }

        await _orderRepository.SaveChangesAsync();

        return Ok(new
        {
            message = "Orders created successfully.",
            totalOrders = createdOrders.Count,
            orders = createdOrders.Select(x => new
            {
                x.Id,
                x.CustomerName,
                priority = x.Priority.ToString(),
                status = x.Status.ToString()
            })
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _orderRepository.GetAllAsync();

        return Ok(orders);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);

        if (order == null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);

        if (order == null)
        {
            return NotFound();
        }

        try
        {
            await _sagaService.CancelOrderAsync(order);

            return Ok(new
            {
                message = "Order cancelled and compensation completed successfully.",
                orderId = order.Id,
                status = order.Status.ToString()
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}