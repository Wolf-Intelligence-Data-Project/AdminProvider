using AdminProvider.OrdersManagement.Models.DTOs;

namespace AdminProvider.OrdersManagement.Models.Responses;

public class OrdersResponseDto
{
    public int TotalCount { get; set; }
    public List<OrderDto> Orders { get; set; }
}