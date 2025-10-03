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

    public async Task<ServiceResult<string>> CreatePaymentIntentAsync(PaymentIntentDto paymentIntentDto)
    {
        try
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = paymentIntentDto.Amount,
                Currency = paymentIntentDto.Currency,
                Customer = paymentIntentDto.CustomerId.ToString(),
                ReceiptEmail = paymentIntentDto.CustomerEmail,
                Description = paymentIntentDto.Description,
                Metadata = new Dictionary<string, string>
                {
                    {"OrderId", paymentIntentDto.OrderId.ToString() },
                    {"CustomerName", paymentIntentDto.CustomerName }
                }
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            // Retorna el client_secret para que el frontend pueda confirmar el pago
            return ServiceResult<string>.Success(paymentIntent.ClientSecret);
        }
        catch (StripeException ex)
        {
            // La mayoría de los errores de Stripe son errores de la petición o de pago (Bad Request, Payment Required)
            // Usamos un 400 Bad Request o 402 Payment Required si se conoce el error de pago específico, 
            // pero 400 es seguro para la mayoría de los errores de la API de Stripe.

            int statusCode = 400;

            if (ex.StripeError != null)
            {
                // Si el error es una falla en el pago (ej. tarjeta rechazada)
                if (ex.StripeError.Type == "card_error")
                {
                    // 402 Payment Required: Indica que el cliente debe pagar (la tarjeta falló, etc.)
                    statusCode = 402;
                }
            }

            return ServiceResult<string>.Failure($"Stripe error: {ex.StripeError.Code} - {ex.Message}", statusCode);
        }
        catch (Exception ex)
        {
            // 500 Internal Server Error: Para errores inesperados no relacionados directamente con la API de Stripe
            return ServiceResult<string>.Failure($"An unexpected error occurred: {ex.Message}", 500);
        }
    }
}