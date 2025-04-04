using AdminProvider.OrdersManagement.Data.Entities;
using AdminProvider.OrdersManagement.Factories;
using AdminProvider.OrdersManagement.Interfaces;
using AdminProvider.OrdersManagement.Models.DTOs;
using AdminProvider.OrdersManagement.Models.Requests;

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

        // Convert OrderEntities to OrderDtos
        var orderDtos = OrderDtoFactory.CreateList(orders);

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