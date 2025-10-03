using API.Dtos.Order;
using API.Helpers; // Necesario para usar ToActionResult()
using API.Helpers.Response;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.Extensions;

namespace API.Controllers;

public class OrdersController : BaseApiController
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService, IStripeService stripeService)
    {
        _orderService = orderService;
    }

    // RUTA 1: CREAR ORDEN
    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)] // Por fallos de Stripe
    public async Task<ActionResult<OrderDto>> Post(OrderAddDto orderDto)
    {
        int userId = User.GetUserId();
        if (userId == -1)
            // Error de autenticación/token, manejado localmente
            return Unauthorized(new ApiResponse(401, "Invalid or missing User ID in token."));

        var result = await _orderService.CreateOrderAsync(orderDto, userId);

        if (result.IsSuccess)
            // Mantenemos la lógica CreatedAtAction para el 201 RESTful
            return CreatedAtAction(nameof(Get), new { id = result.Data.Id },
                                    new ApiResponse<OrderDto>(201, "Order created successfully. Pending payment.", result.Data));

        // Fallo: ToActionResult aplica el StatusCode (ej. 404, 503) y el cuerpo ApiResponse
        return result.ToActionResult();
    }

    // RUTA 2: OBTENER ÓRDENES DEL USUARIO LOGUEADO
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Pager<OrderDto>>> GetOrdersByUser([FromQuery] Params orderParams)
    {
        int userId = User.GetUserId();

        var result = await _orderService.GetOrdersByUserAsync(userId, orderParams);

        // Control total: retorna 200 OK o el error apropiado del Service.
        return result.ToActionResult();
    }

    // RUTA 3: OBTENER UNA ORDEN POR ID
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderDto>> Get(int id)
    {
        var result = await _orderService.GetOrderByIdAsync(id);

        // Control total: retorna 200 OK, 404 Not Found o 500 Internal Server Error
        return result.ToActionResult();
    }

    // RUTA 4: OBTENER TODAS LAS ÓRDENES (SOLO PARA ADMINISTRADORES)
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Pager<OrderDto>>> GetAll([FromQuery] Params orderParams)
    {
        var result = await _orderService.GetOrdersAsync(orderParams);

        // Control total: retorna 200 OK o 500 Internal Server Error
        return result.ToActionResult();
    }

    // RUTA 5: ACTUALIZAR EL ESTADO/DETALLES DE UNA ORDEN (SOLO PARA ADMINISTRADORES)
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Por status inválido
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderDto>> Put(int id, [FromBody] OrderUpdateDto orderDto)
    {
        var result = await _orderService.UpdateOrderAsync(id, orderDto);

        if (result.IsSuccess)
            // Mantenemos la construcción explícita para personalizar el mensaje 200 OK.
            return Ok(new ApiResponse<OrderDto>(200, "Order updated successfully.", result.Data));

        // Fallo: ToActionResult aplica el StatusCode (ej. 400 o 404) y el cuerpo ApiResponse
        return result.ToActionResult();
    }
}