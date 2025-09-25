using API.Dtos.Stripe;
using Core.Services;

namespace API.Services.Interfaces;

public interface IStripeService
{
    Task<ServiceResult<string>> CreatePaymentIntentAsync(PaymentIntentDto paymentIntentDto);
}
