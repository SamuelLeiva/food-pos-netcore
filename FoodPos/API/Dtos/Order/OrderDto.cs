using Core.Enums;

namespace API.Dtos.Order;

public class OrderDto
{
    public int Id { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentIntentId { get; set; }
    public string StripeCustomerId { get; set; }
    public string ReceiptEmail { get; set; }
    public List<OrderItemDto> OrderItems { get; set; }
}
