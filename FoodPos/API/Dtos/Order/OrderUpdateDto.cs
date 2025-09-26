using Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace API.Dtos.Order;

public class OrderUpdateDto
{
    [Required(ErrorMessage = "Order status is required.")]
    [EnumDataType(typeof(OrderStatus), ErrorMessage = "Invalid order status. Must be one of: Pending, Processing, Completed, Shipped, Canceled.")]
    public string Status { get; set; }
}
