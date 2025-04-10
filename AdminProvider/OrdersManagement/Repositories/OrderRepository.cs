using AdminProvider.OrdersManagement.Data;
using AdminProvider.OrdersManagement.Data.Entities;
using AdminProvider.OrdersManagement.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AdminProvider.OrdersManagement.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _orderDbContext;
    private readonly ILogger<OrderRepository> _logger;
    public OrderRepository(OrderDbContext orderDbContext, ILogger<OrderRepository> logger)
    {
        _orderDbContext = orderDbContext;
        _logger = logger;
    }

    public async Task<(List<OrderEntity>, int)> GetAllAsync(int pageNumber, int pageSize)
    {
        var totalCount = await _orderDbContext.Orders.CountAsync();  // Total orders count

        var orders = await _orderDbContext.Orders
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (orders, totalCount);
    }

    public async Task<(List<OrderEntity> Orders, int TotalCount)> SearchAsync(
    string query, int pageNumber, int pageSize)
    {
        _logger.LogInformation("SearchAsync called with parameters: Query = {Query}, PageNumber = {PageNumber}, PageSize = {PageSize}",
                                query, pageNumber, pageSize);

        query = query?.Trim();

        // If query is empty or contains only spaces, fetch all orders
        if (string.IsNullOrWhiteSpace(query))
        {
            _logger.LogInformation("Empty query provided, fetching all orders.");
            return await GetAllOrders(pageNumber, pageSize); // This should return all orders
        }

        var lowerQuery = $"%{query.ToLower()}%";
        _logger.LogInformation("Sanitized query for DB search: {LowerQuery}", lowerQuery);

        var baseQuery = _orderDbContext.Orders
            .Where(order =>
                EF.Functions.Like(order.OrderId.ToString().ToLower(), lowerQuery) ||
                EF.Functions.Like(order.CustomerId.ToString().ToLower(), lowerQuery) ||
                EF.Functions.Like(order.CustomerEmail.ToLower(), lowerQuery));

        var totalCount = await baseQuery.CountAsync();  // Count matching records
        _logger.LogInformation("Total orders found: {TotalCount}", totalCount);

        var orders = await baseQuery
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (orders, totalCount);
    }




    // New method to fetch all orders with pagination
    private async Task<(List<OrderEntity> Orders, int TotalCount)> GetAllOrders(int pageNumber, int pageSize)
    {
        var orders = await _orderDbContext.Orders
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalCount = await _orderDbContext.Orders.CountAsync();

        return (orders, totalCount);
    }


    public async Task<OrderEntity?> GetByOrderIdAsync(Guid orderId)
    {
        return await _orderDbContext.Orders
            .FirstOrDefaultAsync(o => o.OrderId == orderId);
    }
    public async Task<int> GetCountByCustomerIdAsync(Guid customerId)
    {
        return await _orderDbContext.Orders
            .Where(o => o.CustomerId == customerId)
            .CountAsync();
    }
    public async Task<Dictionary<Guid, int>> GetOrderCountsForCustomerIdsAsync(List<Guid> customerIds)
    {
        // Ensure customerIds is not empty
        if (customerIds == null || customerIds.Count == 0)
        {
            return new Dictionary<Guid, int>();
        }

        // Fetch order counts for all customer IDs in one query
        var orderCounts = await _orderDbContext.Orders
            .Where(o => customerIds.Contains(o.CustomerId)) // Filter orders by customer IDs
            .GroupBy(o => o.CustomerId) // Group by CustomerId
            .Select(g => new
            {
                CustomerId = g.Key,
                OrderCount = g.Count()
            })
            .ToListAsync();

        // Convert to a dictionary for easy lookup by customer ID
        return orderCounts.ToDictionary(x => x.CustomerId, x => x.OrderCount);
    }

    public async Task<List<OrderEntity>> GetByCustomerIdAsync(Guid customerId)
    {
        return await _orderDbContext.Orders
            .Where(o => o.CustomerId == customerId && o.PaymentStatus != "Pending") // Exclude pending orders
            .ToListAsync();
    }

    public async Task<List<OrderEntity>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        return await _orderDbContext.Orders
            .Where(o => o.CreatedAt >= fromDate && o.CreatedAt <= toDate) // Filter orders by date range
            .OrderBy(o => o.CreatedAt) // Sort by creation date
            .ToListAsync();
    }

}
