namespace AdminProvider.OrdersManagement.Data.Entities;

public class OrderEntity
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerEmail { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal PricePerProduct { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPriceWithoutVat { get; set; }
    public decimal TotalPrice { get; set; }
    public string PaymentStatus { get; set; }
    public Guid FiltersUsed { get; set; }
    public string? KlarnaPaymentId { get; set; }
}
