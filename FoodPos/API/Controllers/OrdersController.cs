using API.Dtos.Order;
using API.Helpers;
using API.Helpers.Response;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using API.Extensions;

namespace API.Controllers;

public class OrdersController : BaseApiController
{
    private readonly IOrderService _orderService;
    private readonly IStripeService _stripeService;

    public OrdersController(IOrderService orderService, IStripeService stripeService)
    {
        _orderService = orderService;
        _stripeService = stripeService;
    }

    [HttpPost]
    [Authorize] // Requiere que el usuario esté logueado (cliente o admin)
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<OrderDto>>> Post(OrderAddDto orderDto)
    {
        int userId = User.GetUserId();
        if (userId == -1)
            return Unauthorized(new ApiResponse(401, "Invalid or missing User ID in token."));

        var result = await _orderService.CreateOrderAsync(orderDto, userId);

        if (result.IsSuccess)
            return CreatedAtAction(nameof(Get), new { id = result.Data.Id }, new ApiResponse<OrderDto>(201, "Order created successfully. Pending payment.", result.Data));

        return BadRequest(new ApiResponse(400, result.ErrorMessage));
    }

    [HttpGet("me")]
    [Authorize] // Solo usuarios logueados pueden ver sus propias órdenes
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Pager<OrderDto>>>> GetOrdersByUser([FromQuery] Params orderParams)
    {
        // Obtener el ID del usuario logueado desde el token
        int userId = User.GetUserId();

        var result = await _orderService.GetOrdersByUserAsync(userId, orderParams);

        if (result.IsSuccess)
            return Ok(new ApiResponse<Pager<OrderDto>>(200, "User orders retrieved successfully.", result.Data));

        return BadRequest(new ApiResponse(400, result.ErrorMessage));
    }

    [HttpGet("{id}")]
    [Authorize] // Requiere estar logueado
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OrderDto>>> Get(int id)
    {
        var result = await _orderService.GetOrderByIdAsync(id);

        if (result.IsSuccess)
        {
            // LÓGICA DE SEGURIDAD:
            // Asegúrate de que el usuario logueado solo pueda ver sus propias órdenes, 
            // a menos que sea un Administrador (esta verificación debe estar en el Service Layer).
            return Ok(new ApiResponse<OrderDto>(200, "Order retrieved successfully.", result.Data));
        }

        return NotFound(new ApiResponse(404, result.ErrorMessage));
    }

    // RUTA 4: OBTENER TODAS LAS ÓRDENES (SOLO PARA ADMINISTRADORES)
    [HttpGet]
    [Authorize(Roles = "Admin")] // Solo administradores pueden ver todas las órdenes
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<Pager<OrderDto>>>> GetAll([FromQuery] Params orderParams)
    {
        var result = await _orderService.GetOrdersAsync(orderParams);

        if (result.IsSuccess)
            return Ok(new ApiResponse<Pager<OrderDto>>(200, "All orders retrieved successfully.", result.Data));

        return BadRequest(new ApiResponse(400, result.ErrorMessage));
    }

    // RUTA 5: ACTUALIZAR EL ESTADO/DETALLES DE UNA ORDEN (SOLO PARA ADMINISTRADORES)
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")] // Solo administradores pueden actualizar órdenes
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<OrderDto>>> Put(int id, [FromBody] OrderUpdateDto orderDto)
    {
        var result = await _orderService.UpdateOrderAsync(id, orderDto);

        if (result.IsSuccess)
            return Ok(new ApiResponse<OrderDto>(200, "Order updated successfully.", result.Data));

        if (result.ErrorMessage.Contains("does not exist"))
            return NotFound(new ApiResponse(404, result.ErrorMessage));

        return BadRequest(new ApiResponse(400, result.ErrorMessage));
    }
}
