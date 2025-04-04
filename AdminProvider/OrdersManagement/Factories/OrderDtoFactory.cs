using AdminProvider.OrdersManagement.Data.Entities;
using AdminProvider.OrdersManagement.Models.DTOs;

namespace AdminProvider.OrdersManagement.Factories;

public static class OrderDtoFactory
{
    public static OrderDto Create(OrderEntity order)
    {
        return new OrderDto
        {
            OrderId = order.OrderId,
            CustomerId = order.CustomerId,
            CustomerEmail = order.CustomerEmail,
            CreatedAt = order.CreatedAt,
            PricePerProduct = order.PricePerProduct,
            Quantity = order.Quantity,
            TotalPriceWithoutVat = order.TotalPriceWithoutVat,
            TotalPrice = order.TotalPrice,
            PaymentStatus = order.PaymentStatus,
            FiltersUsed = order.FiltersUsed,
            KlarnaPaymentId = order.KlarnaPaymentId
        };
    }
    public static List<OrderDto> CreateList(IEnumerable<OrderEntity> orders)
    {
        return orders.Select(Create).ToList();
    }
}
