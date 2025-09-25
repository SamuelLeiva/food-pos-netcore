using API.Dtos.Stripe;
using API.Helpers.Errors;
using API.Helpers.Response;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class StripeController : BaseApiController
{
    private readonly IStripeService _stripeService;

    public StripeController(IStripeService stripeService)
    {
        _stripeService = stripeService;
    }

    [HttpPost("create-payment-intent")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> CreatePaymentIntent([FromBody] PaymentIntentDto paymentIntentDto)
    {
        // luego añadir validaciones de existencia
        var result = await _stripeService.CreatePaymentIntentAsync(paymentIntentDto);

        if (result.IsSuccess)
            return Ok(new ApiResponse<string>(200, "Payment created successfully.", result.Data));
        
        return BadRequest(new ApiResponse(400, result.ErrorMessage));

        
    }
}
