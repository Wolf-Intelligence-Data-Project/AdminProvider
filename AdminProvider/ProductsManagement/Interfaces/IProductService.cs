namespace AdminProvider.ProductsManagement.Interfaces
{
    public interface IProductService
    {
        Task<int> GetAllProductsCountAsync();
        Task<int> GetUnsoldProductsCountAsync();
        Task<int> GetSoldProductsCountAsync();
        Task ImportProductsFromExcelAsync(IFormFile file);
    }
}