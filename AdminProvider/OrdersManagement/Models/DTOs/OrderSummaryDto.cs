namespace AdminProvider.OrdersManagement.Models.DTOs;

public class OrderSummaryDto
{
    public int TotalOrders { get; set; }
    public int Last24h { get; set; }
    public int Last7d { get; set; }
    public int Last30d { get; set; }
    public int Last6mo { get; set; }
}