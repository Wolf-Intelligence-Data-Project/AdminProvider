using AdminProvider.ProductsManagement.Data.Entities;

namespace AdminProvider.ProductsManagement.Interfaces;

public interface IProductRepository
{
    Task AddProductsAsync(List<ProductEntity> products);

    Task<int> GetTotalProductCountAsync();
    Task<int> GetUnsoldProductCountAsync();
    Task<int> GetSoldProductCountAsync();
}