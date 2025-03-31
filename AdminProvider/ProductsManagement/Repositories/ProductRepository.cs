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

        _logger.LogInformation("Starting batch processing for {ProductCount} products.", products.Count);

        for (int i = 0; i < products.Count; i += batchSize)
        {
            var batch = products.Skip(i).Take(batchSize).ToList();
            _logger.LogInformation("Processing batch {BatchNumber} with {BatchSize} products.", i / batchSize + 1, batch.Count);

            foreach (var product in batch)
            {
                try
                {
                    var existingProduct = await _productDbContext.Products
                        .FirstOrDefaultAsync(p => p.OrganizationNumber == product.OrganizationNumber);

                    if (existingProduct != null)
                    {
                        // Update product fields
                        existingProduct.CompanyName = product.CompanyName;
                        existingProduct.OrganizationNumber = product.OrganizationNumber;
                        existingProduct.Address = product.Address;
                        existingProduct.PostalCode = product.PostalCode;
                        existingProduct.City = product.City;
                        existingProduct.PhoneNumber = product.PhoneNumber;
                        existingProduct.Email = product.Email;
                        existingProduct.BusinessType = product.BusinessType;
                        existingProduct.NumberOfEmployees = product.NumberOfEmployees;
                        existingProduct.CEO = product.CEO;
                        existingProduct.SoldUntil = product.SoldUntil;
                        existingProduct.CustomerId = product.CustomerId;
                        existingProduct.ReservedUntil = product.ReservedUntil;
                        _logger.LogInformation("Updated product with OrganizationNumber: {OrganizationNumber}", product.OrganizationNumber);
                    }
                    else
                    {
                        // Insert new product (ProductId is generated automatically)
                        await _productDbContext.Products.AddAsync(product);
                        _logger.LogInformation("Inserted new product with OrganizationNumber: {OrganizationNumber}", product.OrganizationNumber);
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing product with OrganizationNumber: {OrganizationNumber}", product.OrganizationNumber);
                }
            }

            try
            {
                await _productDbContext.SaveChangesAsync();
                _logger.LogInformation("Successfully saved batch {BatchNumber}.", i / batchSize + 1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving batch {BatchNumber}.", i / batchSize + 1);
            }
        }

        _logger.LogInformation("Finished processing all products.");
    }


    // Method to safely convert values to decimal
    private decimal ConvertToDecimal(object value)
    {
        try
        {
            // Check if the value is already a decimal
            if (value is decimal decimalValue)
            {
                return decimalValue;
            }
            // Check if the value is an int
            else if (value is int intValue)
            {
                return (decimal)intValue;
            }
            // Check if the value is a double
            else if (value is double doubleValue)
            {
                return (decimal)doubleValue;
            }
            // Handle cases where it's an unsupported type (return default 0)
            else
            {
                _logger.LogWarning("Revenue value is of unsupported type, defaulting to 0.");
                return 0m;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting value to decimal.");
            return 0m; // Default to 0 if any errors occur
        }
    }

    private decimal GetRevenueFromReader(SqlDataReader reader, int index)
    {
        var value = reader.GetValue(index); // Get the raw value from the database

        if (value is int intValue)
        {
            return (decimal)intValue; // Convert int to decimal
        }
        else if (value is decimal decimalValue)
        {
            return decimalValue; // Already a decimal, no conversion needed
        }
        else
        {
            // Handle other possible types or throw an exception if necessary
            throw new InvalidCastException("Cannot cast value to decimal");
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
