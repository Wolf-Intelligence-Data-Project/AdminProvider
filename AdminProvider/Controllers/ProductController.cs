using AdminProvider.ProductsManagement.Interfaces;
using AdminProvider.ProductsManagement.Models;
using Microsoft.AspNetCore.Mvc;

namespace AdminProvider.Controllers;

[ApiController]
[Route("api/product")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductController> _logger;

    public ProductController(IProductService productService, ILogger<ProductController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Get the total count of all products.
    /// </summary>
    [HttpGet("count")]
    public async Task<IActionResult> GetTotalProductsCount()
    {
        try
        {
            // Assuming _productService.GetProductsCountAsync() returns an object with total, unsold, and sold counts
            var count = await _productService.GetProductsCountAsync();

            // Create a ProductsCountResponse instance with the retrieved data
            var response = new ProductsCountResponse
            {
                TotalProductsCount = count.TotalProductsCount,
                UnsoldProductsCount = count.UnsoldProductsCount,
                SoldProductsCount = count.SoldProductsCount
            };

            _logger.LogInformation("Product counts retrieved: Total = {Total}, Unsold = {Unsold}, Sold = {Sold}",
          response.TotalProductsCount, response.UnsoldProductsCount, response.SoldProductsCount);

            // Return the response model
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching total product count.");
            return StatusCode(500, new { Message = "An error occurred while retrieving product count." });
        }
    }

    /// <summary>
    /// Import products from an Excel file.
    /// </summary>
    [HttpPost("import-excel")]
    public async Task<IActionResult> ImportProductsFromExcel([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { Message = "The uploaded file is empty or invalid." });
        }

        try
        {
            await _productService.ImportProductsFromExcelAsync(file);
            return Ok(new { Message = "Products imported successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while importing products from Excel.");
            return StatusCode(500, new { Message = "An error occurred while importing products." });
        }
    }
}
