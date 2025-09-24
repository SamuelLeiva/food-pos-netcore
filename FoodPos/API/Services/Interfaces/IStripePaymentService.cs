//using API.Dtos.Stripe;
//using Core.Services;

//namespace API.Services.Interfaces;

//public interface IStripePaymentService
//{
//    // Crea una sesión de pago y devuelve la URL para redirigir al usuario
//    Task<ServiceResult<string>> CreateCheckoutSessionAsync(List<CheckoutProductDto> productIds);

//    // Maneja los eventos de webhook de Stripe
//    Task<ServiceResult> HandleWebhookAsync(string json, string signature);
//}
