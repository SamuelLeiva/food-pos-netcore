using System.ComponentModel.DataAnnotations;

namespace API.Dtos.Stripe;

public class PaymentIntentDto
{
    [Required(ErrorMessage = "Amount cannot be empty.")]
    [Range(1, long.MaxValue, ErrorMessage = "Amount must be a positive value.")]
    public long Amount { get; set; }
    // the currency should be validated against a list of currencies
    [Required(ErrorMessage = "Currency cannot be empty.")]
    [RegularExpression("^[a-z]{3}$", ErrorMessage = "Currency must be a valid 3-letter ISO code.")]
    public string Currency { get; set; } = "usd";
    [Required(ErrorMessage = "Customer Id cannot be empty.")]
    public string CustomerId { get; set; }
    [Required(ErrorMessage = "Customer email cannot be empty.")]
    [EmailAddress(ErrorMessage = "The email format is invalid.")]
    public string CustomerEmail { get; set; }
    [Required(ErrorMessage = "Customer name cannot be empty.")]
    [StringLength(100, ErrorMessage = "Customer name cannot be longer than 100 characters.")]
    public string CustomerName {  get; set; }
    [StringLength(200, ErrorMessage = "Description cannot be longer than 200 characters.")]
    public string Description { get; set; }
    [Required(ErrorMessage = "Order Id cannot be empty.")]
    [Range(1, int.MaxValue, ErrorMessage = "Order Id must be a positive value.")]
    public int OrderId { get; set; }
}
