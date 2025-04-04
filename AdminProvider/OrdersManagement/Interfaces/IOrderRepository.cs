using AdminProvider.OrdersManagement.Data.Entities;
using AdminProvider.OrdersManagement.Models.DTOs;

namespace AdminProvider.OrdersManagement.Interfaces;

public interface IOrderRepository
{
    Task<(List<OrderEntity>, int TotalCount)> GetAllAsync(int pageNumber, int pageSize);

    Task<OrderEntity?> GetByOrderIdAsync(Guid orderId);

    Task<List<OrderEntity>> GetByCustomerIdAsync(Guid customerId);

    Task<List<OrderEntity>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
}
