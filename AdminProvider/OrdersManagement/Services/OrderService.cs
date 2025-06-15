using AdminProvider.OrdersManagement.Data.Entities;
using AdminProvider.OrdersManagement.Factories;
using AdminProvider.OrdersManagement.Interfaces;
using AdminProvider.OrdersManagement.Models.DTOs;
using AdminProvider.OrdersManagement.Models.Requests;
using System.Globalization;

namespace AdminProvider.OrdersManagement.Services;


public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IOrderRepository orderRepository, ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<(List<OrderDto> Orders, int TotalCount)> GetAllOrdersAsync(int pageNumber, int pageSize)
    {
        var (orders, totalCount) = await _orderRepository.GetAllAsync(pageNumber, pageSize);

        var orderDtos = OrderDtoFactory.CreateList(orders);

        return (orderDtos, totalCount);
    }
    public async Task<OrderSummaryDto> GetOrderSummaryAsync()
    {
        var now = DateTime.UtcNow;

        var total = await _orderRepository.CountAllOrdersAsync();

        // Last 24 Hours
        var last24h = await _orderRepository.CountOrdersSinceAsync(now.AddHours(-24));

        // Last 7 Days (exclude Last 24h)
        var last7d = await _orderRepository.CountOrdersSinceAsync(now.AddDays(-7), now.AddHours(-24));

        // Last 30 Days (exclude Last 7 days)
        var last30d = await _orderRepository.CountOrdersSinceAsync(now.AddDays(-30), now.AddDays(-7));

        // Last 6 Months (exclude Last 30 days)
        var last6mo = await _orderRepository.CountOrdersSinceAsync(now.AddMonths(-6), now.AddDays(-30));

        return new OrderSummaryDto
        {
            TotalOrders = total,
            Last24h = last24h,
            Last7d = last7d,
            Last30d = last30d,
            Last6mo = last6mo
        };
    }

    public async Task<(List<OrderDto> Orders, int TotalCount)> SearchOrdersAsync(SearchRequest request)
    {
        _logger.LogInformation("SearchOrdersAsync called with parameters: Query = {Query}, PageNumber = {PageNumber}, PageSize = {PageSize}",
                               request.Query, request.PageNumber, request.PageSize);

        var (orders, totalCount) = await _orderRepository.SearchAsync(
            request.Query,
            request.PageNumber,
            request.PageSize,
            request.StartDate,
            request.EndDate,
            request.SortCriteria
        );

        _logger.LogInformation("SearchOrdersAsync retrieved {OrderCount} orders, TotalCount: {TotalCount}",
                               orders.Count, totalCount);

        var orderDtos = OrderDtoFactory.CreateList(orders);

        _logger.LogInformation("Converted {OrderCount} OrderEntities to OrderDtos.", orderDtos.Count);

        return (orderDtos, totalCount);
    }



    public async Task<OrderEntity?> GetOrderByIdAsync(OrderRequest request)
    {
        if (request.Id == null)
        {
            throw new ArgumentNullException(nameof(request.Id), "Order request cannot be null.");
        }

        if (!Guid.TryParse(request.Id, out Guid userId))
        {
            throw new ArgumentException("Invalid Order ID format.", nameof(request.Id));
        }

        var order = await _orderRepository.GetByOrderIdAsync(userId);

        return order;
    }

    public async Task<List<OrderEntity>> GetOrdersByCustomerIdAsync(OrderRequest request)
    {
        if (string.IsNullOrEmpty(request.Id)) 
        {
            throw new ArgumentNullException(nameof(request.Id), "Customer ID cannot be null or empty.");
        }

        if (!Guid.TryParse(request.Id, out Guid customerId)) 
        {
            throw new ArgumentException("Invalid Customer ID format.", nameof(request.Id));
        }

        var orders = await _orderRepository.GetByCustomerIdAsync(customerId); 

        return orders;
    }

    public async Task<List<OrderEntity>> GetOrdersByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        try
        {
            return await _orderRepository.GetByDateRangeAsync(fromDate, toDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching orders between {FromDate} and {ToDate}.", fromDate, toDate);
            throw new Exception("An error occurred while retrieving orders by date range.");
        }
    }
}