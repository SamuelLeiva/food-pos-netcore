using API.Helpers.Errors;
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<string>> CreatePaymentIntent([FromBody] long amount)
    {
        var result = await _stripeService.CreatePaymentIntentAsync(amount);

        if (result.IsSuccess)
            // Retorna un Ok con el client_secret en caso de éxito
            return Ok(result.Data);

        // Retorna un BadRequest con el mensaje de error si la operación falló
        return BadRequest(new ApiResponse(400, result.ErrorMessage));
    }
}
