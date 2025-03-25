using AdminProvider.OrdersManagement.Data;
using AdminProvider.OrdersManagement.Data.Entities;
using AdminProvider.OrdersManagement.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AdminProvider.OrdersManagement.Models;

public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _orderDbContext;
    private readonly ILogger<OrderRepository> _logger;
    public OrderRepository(OrderDbContext orderDbContext, ILogger<OrderRepository> logger)
    {
        _orderDbContext = orderDbContext;
        _logger = logger;
    }

    public async Task<List<OrderEntity>> GetAllAsync()
    {
        return await _orderDbContext.Orders
            .Where(o => o.PaymentStatus != "Pending") // Exclude orders with "Pending" status
            .ToListAsync();
    }

    public async Task<OrderEntity?> GetByOrderIdAsync(Guid orderId)
    {
        return await _orderDbContext.Orders
            .FirstOrDefaultAsync(o => o.OrderId == orderId);
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
