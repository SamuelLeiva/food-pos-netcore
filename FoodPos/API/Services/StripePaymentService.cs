//using API.Dtos.Products;
//using API.Dtos.Stripe;
//using API.Services.Interfaces;
//using AutoMapper;
//using Core.Interfaces;
//using Core.Services;
//using Stripe.Checkout;

//namespace API.Services;

//public class StripePaymentService : IStripePaymentService
//{
//    private readonly IUnitOfWork _unitOfWork;

//    public StripePaymentService(IUnitOfWork unitOfWork, IMapper mapper)
//    {
//        _unitOfWork = unitOfWork;
//    }
//    public async Task<string> CreateCheckoutSessionAsync(List<CheckoutProductDto> products)
//    {
//        // 1. Obtiene los productos de tu base de datos para validar precios
//        var lineItems = new List<SessionLineItemOptions>();
//        foreach (var productDto in products)
//        {
//            var product = await _unitOfWork.Products.GetByIdAsync(productDto.ProductId);
//            if (product != null)
//            {
//                lineItems.Add(new SessionLineItemOptions
//                {
//                    PriceData = new SessionLineItemPriceDataOptions
//                    {
//                        UnitAmountDecimal = (decimal?)product.Price, //Stripe usa centavos
//                        Currency = "usd",
//                        ProductData = new SessionLineItemPriceDataProductDataOptions
//                        {
//                            Name = product.Name,
//                            Description = product.Description
//                        }
//                    },
//                    Quantity = productDto.Quantity
//                });
//            }
//        }

//        if (!lineItems.Any())
//        {
//            return ServiceResult<string>.Failure("No valid products found for the payment session.");
//        }

//        // 2. Define las opciones de la sesión
//        var options = new SessionCreateOptions
//        {
//            SuccessUrl = "", // URL de mi frontend
//            CancelUrl = "", // URL de cancelación de mi frontend,
//            Mode = "payment",
//            LineItems = lineItems
//        };

//        // 3. Crea la sesión de Stripe
//        var service = new SessionService();
//        try
//        {
//            var session = await service.CreateAsync(options);
//            return ServiceResult<string>.Success(session.Url);
//        }
//        catch (Exception ex)
//        {
//            Console.Write(ex.ToString());
//        }
//    }

//    public Task<bool> HandleWebhookAsync(string json, string signature)
//    {
//        throw new NotImplementedException();
//    }
//}
