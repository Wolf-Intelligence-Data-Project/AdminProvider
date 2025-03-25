using AdminProvider.OrdersManagement.Data.Entities;

namespace AdminProvider.OrdersManagement.Interfaces;

public interface IOrderRepository
{
    Task<List<OrderEntity>> GetAllAsync();

    Task<OrderEntity?> GetByOrderIdAsync(Guid orderId);

    Task<List<OrderEntity>> GetByCustomerIdAsync(Guid customerId);

    Task<List<OrderEntity>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
}
