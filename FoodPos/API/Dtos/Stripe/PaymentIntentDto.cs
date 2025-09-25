namespace API.Dtos.Stripe;

public class PaymentIntentDto
{
    public long Amount { get; set; }
    public string Currency { get; set; } = "usd";
    public string CustomerId { get; set; }
    public string CustomerEmail { get; set; }
    public string CustomerName {  get; set; }
    public string Description { get; set; }
    public int OrderId { get; set; }
}
