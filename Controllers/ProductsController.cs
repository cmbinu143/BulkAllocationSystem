using BulkAllocation.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BulkAllocation.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepository;

    public ProductsController(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _productRepository.GetAllAsync();

        var result = products.Select(product => new
        {
            product.Id,
            product.Name,
            product.SKU,
            product.Price,
            inventory = product.Inventory == null
                ? null
                : new
                {
                    product.Inventory.Id,
                    product.Inventory.ProductId,
                    product.Inventory.AvailableQuantity,
                    product.Inventory.ReservedQuantity
                }
        });

        return Ok(result);
    }
}