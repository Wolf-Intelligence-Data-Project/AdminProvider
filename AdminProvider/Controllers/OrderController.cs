using AdminProvider.OrdersManagement.Interfaces;
using AdminProvider.OrdersManagement.Models.Requests;
using AdminProvider.UsersManagement.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AdminProvider.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderController> _logger;

    public OrderController(IOrderService orderService, ILogger<OrderController> logger)
    {
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all orders.
    /// </summary>
    [HttpGet("get-all")]
    public async Task<IActionResult> GetAllOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var (orderDtos, totalCount) = await _orderService.GetAllOrdersAsync(pageNumber, pageSize);
            if (orderDtos == null)
            {
                _logger.LogInformation("No orders found in result.");
            }
            else
            {
                _logger.LogInformation("Fetched Orders: {@Orders}", orderDtos);
            }

            _logger.LogInformation("Total Orders Count: {TotalCount}", totalCount);

            _logger.LogInformation("Result from GetAllOrdersAsync: {@Result}", orderDtos);

            if (orderDtos == null || totalCount == 0)
            {
                return NotFound(new { Message = "No orders found." });
            }

            var result = new
            {
                Orders = orderDtos,
                TotalCount = totalCount,
            };

            _logger.LogInformation("Result: {Result}", JsonConvert.SerializeObject(result));

            return Ok(result);

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
    [HttpGet("order")]
    public async Task<IActionResult> GetOrderById([FromQuery] OrderRequest request)
    {
        try
        {
            var order = await _orderService.GetOrderByIdAsync(request);
            return Ok(order);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Order with ID {OrderId} not found.");
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
    /// <param name="request">The request model containing the customer ID.</param>
    [HttpGet("orders")]
    public async Task<IActionResult> GetOrdersByCustomerId([FromQuery] OrderRequest request)
    {
        try
        {
            var orders = await _orderService.GetOrdersByCustomerIdAsync(request);
            if (orders == null || orders.Count == 0)
            {
                return NotFound(new { Message = "No orders found for this customer." });
            }
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders for customer ID {CustomerId}.");
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
    [HttpPost("search")]
    public async Task<IActionResult> SearchOrders([FromBody] SearchRequest request)
    {
        _logger.LogInformation("Received search request: Query: {Query}, PageNumber: {PageNumber}, PageSize: {PageSize}, StartDate: {StartDate}, EndDate: {EndDate}, SortCriteria: {SortCriteria}",
                               request.Query, request.PageNumber, request.PageSize, request.StartDate, request.EndDate, request.SortCriteria);

        try
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                request.Query = ""; 
            }

            var (orderDtos, totalCount) = await _orderService.SearchOrdersAsync(request);

            if (orderDtos == null || totalCount == 0)
            {
                return NotFound(new { Message = "No orders found." });
            }

            return Ok(new
            {
                Orders = orderDtos,
                TotalCount = totalCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders.");
            return StatusCode(500, new { Message = "An error occurred while retrieving orders." });
        }
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetOrderSummary()
    {
        var summary = await _orderService.GetOrderSummaryAsync();

        _logger.LogInformation("Returning order summary: {@Summary}", summary);

        return Ok(summary);
    }


}
