using System.ComponentModel.DataAnnotations;

namespace API.Dtos.Order;

public class OrderItemAddUpdateDto
{
    [Required(ErrorMessage = "Product Id is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Product Id must be a positive value.")]
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Quantity is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }
}
