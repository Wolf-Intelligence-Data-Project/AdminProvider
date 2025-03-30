using AdminProvider.ProductsManagement.Models;

namespace AdminProvider.ProductsManagement.Interfaces
{
    public interface IProductService
    {
        Task<ProductsCountResponse> GetProductsCountAsync();
        Task ImportProductsFromExcelAsync(IFormFile file);
    }
}