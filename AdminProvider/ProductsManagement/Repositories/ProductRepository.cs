using System.Data;
using Dapper;
using AdminProvider.ProductsManagement.Data.Entities;
using AdminProvider.ProductsManagement.Data;
using AdminProvider.ProductsManagement.Interfaces;
using Microsoft.Data.SqlClient;
using AdminProvider.ProductsManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminProvider.ProductsManagement.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ProductDbContext _productDbContext;
    private readonly string _connectionString;
    private readonly ILogger<ProductRepository> _logger;

    public ProductRepository(ProductDbContext productDbContext, IConfiguration configuration, ILogger<ProductRepository> logger)
    {
        _productDbContext = productDbContext;
        _connectionString = configuration.GetConnectionString("ProductDatabase");
        _logger = logger;
    }

    public async Task AddProductsAsync(List<ProductEntity> products)
    {
        const int batchSize = 1000; 
        for (int i = 0; i < products.Count; i += batchSize)
        {
            var batch = products.Skip(i).Take(batchSize).ToList();

            foreach (var product in batch)
            {
                // Check if product with the same IdentificationNumber exists
                var existingProduct = await _productDbContext.Products
                    .FirstOrDefaultAsync(p => p.OrganizationNumber == product.OrganizationNumber);

                if (existingProduct != null)
                {
                    // If product exists, update the existing one
                    _productDbContext.Entry(existingProduct).CurrentValues.SetValues(product);
                }
                else
                {
                    // If product doesn't exist, add new one
                    await _productDbContext.Products.AddAsync(product);
                }
            }

            // Save changes in batches
            await _productDbContext.SaveChangesAsync();
        }
    }

    public async Task<ProductsCountResponse> GetProductCountAsync()
    {
        const string sql = @"
    SELECT 
        COUNT(*) AS TotalProductsCount,
        SUM(CASE WHEN SoldUntil IS NULL AND CustomerId IS NULL THEN 1 ELSE 0 END) AS UnsoldProductsCount,
        SUM(CASE WHEN SoldUntil IS NOT NULL OR CustomerId IS NOT NULL THEN 1 ELSE 0 END) AS SoldProductsCount
    FROM Products";

        using var connection = new SqlConnection(_connectionString);

        // Fetch and map the result to ProductsCountResponse
        var result = await connection.QueryFirstOrDefaultAsync<ProductsCountResponse>(sql);

        // Handle case if no data is returned (e.g., empty table)
        return result ?? new ProductsCountResponse
        {
            TotalProductsCount = 0,
            UnsoldProductsCount = 0,
            SoldProductsCount = 0
        };
    }
}
