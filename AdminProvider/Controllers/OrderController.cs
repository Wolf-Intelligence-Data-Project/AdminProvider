using AdminProvider.OrdersManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdminProvider.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly OrderService _orderService;
    private readonly ILogger<OrderController> _logger;

    public OrderController(OrderService orderService, ILogger<OrderController> logger)
    {
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all orders.
    /// </summary>
    [HttpGet("all")]
    public async Task<IActionResult> GetAllOrders()
    {
        try
        {
            var orders = await _orderService.GetAllOrdersAsync();
            if (orders == null || orders.Count == 0)
            {
                return NotFound(new { Message = "No orders found." });
            }
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all orders.");
            return StatusCode(500, new { Message = "An error occurred while retrieving orders." });
        }
    }

    /// <summary>
    /// Retrieves an order by its ID.
    /// </summary>
    /// <param name="orderId">The order ID.</param>
    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> GetOrderById(Guid orderId)
    {
        try
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            return Ok(order);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Order with ID {OrderId} not found.", orderId);
            return NotFound(new { ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order by ID.");
            return StatusCode(500, new { Message = "An error occurred while retrieving the order." });
        }
    }

    /// <summary>
    /// Retrieves orders for a specific customer.
    /// </summary>
    /// <param name="customerId">The customer ID.</param>
    [HttpGet("customer/{customerId:guid}")]
    public async Task<IActionResult> GetOrdersByCustomerId(Guid customerId)
    {
        try
        {
            var orders = await _orderService.GetOrdersByCustomerIdAsync(customerId);
            if (orders == null || orders.Count == 0)
            {
                return NotFound(new { Message = "No orders found for this customer." });
            }
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders for customer ID {CustomerId}.", customerId);
            return StatusCode(500, new { Message = "An error occurred while retrieving customer orders." });
        }
    }

    /// <summary>
    /// Retrieves orders within a date range.
    /// </summary>
    /// <param name="fromDate">The start date.</param>
    /// <param name="toDate">The end date.</param>
    [HttpGet("date-range")]
    public async Task<IActionResult> GetOrdersByDateRange([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        if (fromDate > toDate)
        {
            return BadRequest(new { Message = "FromDate cannot be later than ToDate." });
        }

        try
        {
            var orders = await _orderService.GetOrdersByDateRangeAsync(fromDate, toDate);
            if (orders == null || orders.Count == 0)
            {
                return NotFound(new { Message = "No orders found in the specified date range." });
            }
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders by date range.");
            return StatusCode(500, new { Message = "An error occurred while retrieving orders by date range." });
        }
    }
}
