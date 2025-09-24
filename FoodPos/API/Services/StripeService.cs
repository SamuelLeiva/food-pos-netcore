using API.Dtos.Stripe;
using API.Services.Interfaces;
using Core.Services;
using Microsoft.Extensions.Options;
using Stripe;

namespace API.Services;

public class StripeService : IStripeService
{
    private readonly StripeOptions _stripeOptions;

    public StripeService(IOptions<StripeOptions> stripeOptions)
    {
        _stripeOptions = stripeOptions.Value;
        StripeConfiguration.ApiKey = _stripeOptions.SecretKey;
    }
    public async Task<ServiceResult<string>> CreatePaymentIntentAsync(long amount)
    {
        try
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = amount,
                Currency = "usd", // otras opciones si fuesen necesarias
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            // Retorna el client_secret para que el frontend pueda confirmar el pago
            return ServiceResult<string>.Success(paymentIntent.ClientSecret);
        }
        catch (StripeException ex)
        {
            return ServiceResult<string>.Failure($"Stripe error: {ex.StripeError.Code} - {ex.Message}");
        }
        catch (Exception ex)
        {
            return ServiceResult<string>.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }
}
