using AdminProvider.OrdersManagement.Data.Entities;
using AdminProvider.OrdersManagement.Models.DTOs;
using AdminProvider.OrdersManagement.Models.Requests;

namespace AdminProvider.OrdersManagement.Interfaces;

public interface IOrderRepository
{
    Task<(List<OrderEntity>, int TotalCount)> GetAllAsync(int pageNumber, int pageSize);

    Task<OrderEntity?> GetByOrderIdAsync(Guid orderId);

    Task<List<OrderEntity>> GetByCustomerIdAsync(Guid customerId);
    Task<int> GetCountByCustomerIdAsync(Guid customerId);
    Task<Dictionary<Guid, int>> GetOrderCountsForCustomerIdsAsync(List<Guid> customerIds);
    Task<(List<OrderEntity> Orders, int TotalCount)> SearchAsync(
        string query, int pageNumber, int pageSize, string? startDate, string? endDate, List<SortCriteria> sortCriteria);

    Task<int> CountOrdersSinceAsync(DateTime since, DateTime? until = null);
    Task<int> CountAllOrdersAsync();
    Task<List<OrderEntity>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
}
