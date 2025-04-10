using AdminProvider.OrdersManagement.Data.Entities;
using AdminProvider.OrdersManagement.Models.DTOs;
using AdminProvider.OrdersManagement.Models.Requests;

namespace AdminProvider.OrdersManagement.Interfaces
{
    public interface IOrderService
    {
        Task<(List<OrderDto> Orders, int TotalCount)> GetAllOrdersAsync(int pageNumber, int pageSize);
        Task<OrderEntity?> GetOrderByIdAsync(OrderRequest request);
        Task<List<OrderEntity>> GetOrdersByCustomerIdAsync(OrderRequest request);
        Task<(List<OrderDto> Orders, int TotalCount)> SearchOrdersAsync(string query, int pageNumber, int pageSize);
        Task<List<OrderEntity>> GetOrdersByDateRangeAsync(DateTime fromDate, DateTime toDate);
    }
}