using Microsoft.AspNetCore.Mvc;
using Task3.Services;

namespace IntegratorIntoAspNet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Creating new product.
    /// </summary>
    /// <param name="name">Product name</param>
    /// <param name="price">Product price</param>
    /// <returns>Created product's ID</returns>
    [HttpPost]
    public async Task<ActionResult> CreateProductAsync([FromQuery] string name, [FromQuery] decimal price)
    {
        if (string.IsNullOrEmpty(name))
            return BadRequest("Name of product is required");
        if (decimal.IsNegative(price))
            return BadRequest("Price must be >= 0");
        long productId = await _productService.CreateProduct(name, price);
        return Ok(productId);
    }
}