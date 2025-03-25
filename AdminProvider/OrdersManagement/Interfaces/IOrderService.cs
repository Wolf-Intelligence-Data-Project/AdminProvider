using AdminProvider.OrdersManagement.Data.Entities;

namespace AdminProvider.OrdersManagement.Interfaces
{
    public interface IOrderService
    {
        Task<List<OrderEntity>> GetAllOrdersAsync();
        Task<OrderEntity?> GetOrderByIdAsync(Guid orderId);
        Task<List<OrderEntity>> GetOrdersByCustomerIdAsync(Guid customerId);

        Task<List<OrderEntity>> GetOrdersByDateRangeAsync(DateTime fromDate, DateTime toDate);
    }
}