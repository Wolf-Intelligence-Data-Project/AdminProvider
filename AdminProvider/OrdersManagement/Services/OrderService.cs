using AdminProvider.OrdersManagement.Data.Entities;
using AdminProvider.OrdersManagement.Interfaces;

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

    public async Task<List<OrderEntity>> GetAllOrdersAsync()
    {
        try
        {
            return await _orderRepository.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all orders.");
            throw new Exception("An error occurred while retrieving orders.");
        }
    }

    public async Task<OrderEntity?> GetOrderByIdAsync(Guid orderId)
    {
        try
        {
            var order = await _orderRepository.GetByOrderIdAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning("Order with ID {OrderId} not found.", orderId);
                throw new KeyNotFoundException($"Order with ID {orderId} not found.");
            }
            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching order by ID.");
            throw new Exception("An error occurred while retrieving the order.");
        }
    }

    public async Task<List<OrderEntity>> GetOrdersByCustomerIdAsync(Guid customerId)
    {
        try
        {
            return await _orderRepository.GetByCustomerIdAsync(customerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching orders for customer ID {CustomerId}.", customerId);
            throw new Exception("An error occurred while retrieving customer orders.");
        }
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