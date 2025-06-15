using AdminProvider.OrdersManagement.Data;
using AdminProvider.OrdersManagement.Data.Entities;
using AdminProvider.OrdersManagement.Interfaces;
using AdminProvider.OrdersManagement.Models.Requests;
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
        var totalCount = await _orderDbContext.Orders.CountAsync(); 

        var orders = await _orderDbContext.Orders
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (orders, totalCount);
    }
    public async Task<(List<OrderEntity> Orders, int TotalCount)> SearchAsync(
        string query,
        int pageNumber,
        int pageSize,
        string startDate,
        string endDate,
        List<SortCriteria> sortCriteria)
    {
        var queryable = _orderDbContext.Orders.AsQueryable();

        _logger.LogInformation("Query received for search: {Query}", query);

        if (sortCriteria != null && sortCriteria.Any())
        {
            IOrderedQueryable<OrderEntity>? orderedQuery = null;

            foreach (var (sort, index) in sortCriteria.Select((s, i) => (s, i)))
            {
                var sortBy = sort.SortBy;
                var sortDirection = sort.SortDirection.ToLower() == "desc" ? "desc" : "asc";

                if (_orderDbContext.Model.FindEntityType(typeof(OrderEntity))
                                         .GetProperties()
                                         .Any(p => p.Name == sortBy))
                {
                    if (index == 0)
                    {
                        orderedQuery = sortDirection == "asc"
                            ? queryable.OrderBy(e => EF.Property<object>(e, sortBy))
                            : queryable.OrderByDescending(e => EF.Property<object>(e, sortBy));
                    }
                    else if (orderedQuery != null)
                    {
                        orderedQuery = sortDirection == "asc"
                            ? orderedQuery.ThenBy(e => EF.Property<object>(e, sortBy))
                            : orderedQuery.ThenByDescending(e => EF.Property<object>(e, sortBy));
                    }
                }
                else
                {
                    _logger.LogWarning($"Invalid sort property: {sortBy}. Skipping sorting for this field.");
                }
            }

            if (orderedQuery != null)
            {
                queryable = orderedQuery;
            }
        }
        else
        {

            queryable = queryable.OrderByDescending(o => o.CreatedAt);
        }

        if (string.IsNullOrWhiteSpace(query) || query == "*")
        {
            _logger.LogInformation("Fetching all orders (no query filter applied).");
        }
        else
        {
            // If query is provided, filter based on query
            queryable = queryable.Where(o =>
                o.OrderId.ToString().Contains(query) ||
                o.CustomerId.ToString().Contains(query));
        }

        if (!string.IsNullOrEmpty(startDate))
        {
            queryable = queryable.Where(o => o.CreatedAt >= DateTime.Parse(startDate));
        }

        if (!string.IsNullOrEmpty(endDate))
        {
            queryable = queryable.Where(o => o.CreatedAt <= DateTime.Parse(endDate));
        }

        var totalCount = await queryable.CountAsync();

        // Apply pagination
        var orders = await queryable.Skip((pageNumber - 1) * pageSize)
                                     .Take(pageSize)
                                     .ToListAsync();

        return (orders, totalCount);
    }


    public async Task<int> CountOrdersSinceAsync(DateTime since, DateTime? until = null)
    {
        var query = _orderDbContext.Orders.Where(o => o.CreatedAt >= since);

        if (until.HasValue)
        {
            query = query.Where(o => o.CreatedAt < until.Value);
        }

        return await query.CountAsync();
    }

    public async Task<int> CountAllOrdersAsync()
    {
        return await _orderDbContext.Orders.CountAsync();
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
        if (customerIds == null || customerIds.Count == 0)
        {
            return new Dictionary<Guid, int>();
        }

        var orderCounts = await _orderDbContext.Orders
            .Where(o => customerIds.Contains(o.CustomerId))
            .GroupBy(o => o.CustomerId)
            .Select(g => new
            {
                CustomerId = g.Key,
                OrderCount = g.Count()
            })
            .ToListAsync();

        return orderCounts.ToDictionary(x => x.CustomerId, x => x.OrderCount);
    }

    public async Task<List<OrderEntity>> GetByCustomerIdAsync(Guid customerId)
    {
        return await _orderDbContext.Orders
            .Where(o => o.CustomerId == customerId && o.PaymentStatus != "Pending")
            .ToListAsync();
    }

    public async Task<List<OrderEntity>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        return await _orderDbContext.Orders
            .Where(o => o.CreatedAt >= fromDate && o.CreatedAt <= toDate)
            .OrderBy(o => o.CreatedAt)
            .ToListAsync();
    }

}
