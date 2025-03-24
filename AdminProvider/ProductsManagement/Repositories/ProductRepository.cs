using System.Data;
using Dapper;
using AdminProvider.ProductsManagement.Data.Entities;
using AdminProvider.ProductsManagement.Data;
using AdminProvider.ProductsManagement.Interfaces;
using Microsoft.Data.SqlClient;

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
        await _productDbContext.Products.AddRangeAsync(products);
        await _productDbContext.SaveChangesAsync();
    }

    public async Task<int> GetTotalProductCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM Products";

        using var connection = new SqlConnection(_connectionString);
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<int> GetUnsoldProductCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM Products WHERE SoldUntil IS NULL AND SoldTo IS NULL";

        using var connection = new SqlConnection(_connectionString);
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<int> GetSoldProductCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM Products WHERE SoldUntil IS NOT NULL AND SoldTo IS NOT NULL";

        using var connection = new SqlConnection(_connectionString);
        return await connection.ExecuteScalarAsync<int>(sql);
    }
}
