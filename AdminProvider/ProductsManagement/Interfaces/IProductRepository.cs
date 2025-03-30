using AdminProvider.ProductsManagement.Data.Entities;
using AdminProvider.ProductsManagement.Models;

namespace AdminProvider.ProductsManagement.Interfaces;

public interface IProductRepository
{
    Task AddProductsAsync(List<ProductEntity> products);

    Task<ProductsCountResponse> GetProductCountAsync();

}