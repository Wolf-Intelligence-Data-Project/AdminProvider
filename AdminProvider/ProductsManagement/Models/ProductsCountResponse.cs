namespace AdminProvider.ProductsManagement.Models;

public class ProductsCountResponse
{
    public int TotalProductsCount { get; set; }
    public int UnsoldProductsCount { get; set; }
    public int SoldProductsCount { get; set; }
}
