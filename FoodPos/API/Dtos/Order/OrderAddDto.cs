using Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace API.Dtos.Order;

public class OrderAddDto
{
    [Required(ErrorMessage = "The OrderItems list cannot be empty.")]
    [MinLength(1, ErrorMessage = "An order must contain at least one item.")]
    public List<OrderItemAddUpdateDto> OrderItems { get; set; }

    [Required(ErrorMessage = "Receipt email is required.")]
    [EmailAddress(ErrorMessage = "The email format is invalid.")]
    public string ReceiptEmail { get; set; }

    [Required(ErrorMessage = "Order status is required.")]
    [EnumDataType(typeof(OrderStatus), ErrorMessage = "Invalid order status. Must be one of: Pending, Processing, Completed, Shipped, Canceled.")]
    public string Status { get; set; }
}
