using API.Dtos.Users;
using API.Helpers.Response;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.Extensions; // Asegúrate de que esta extensión ToActionResult() esté aquí

namespace API.Controllers;

public class UsersController : BaseApiController
{
    private readonly IUserService _userService;
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    // 1. Registro de usuario
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> RegisterAsync(RegisterDto model)
    {
        var result = await _userService.RegisterAsync(model);

        if (result.IsSuccess)
            // Retorna 201 Created para éxito
            return new CreatedResult(string.Empty, new ApiResponse(201, "User registered successfully."));

        // Si falla, ToActionResult usará el 409 Conflict o el 500 del service.
        return result.ToActionResult();
    }

    // 2. Obtener Token (Login)
    [HttpPost("token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDataDto>> GetTokenAsync(LoginDto model)
    {
        var result = await _userService.GetTokenAsync(model);

        if (result.IsSuccess)
        {
            SetRefreshTokenInCookie(result.Data.RefreshToken);
            return Ok(new ApiResponse<UserDataDto>(200, "Token generated successfully.", result.Data));
        }

        // Si falla, ToActionResult usará el 401 Unauthorized, 404 Not Found o 500 del service.
        return result.ToActionResult();
    }

    // 3. Agregar Rol (Solo para Admin en producción)
    // [Authorize(Roles = "Admin")] // Descomentar en producción
    [HttpPost("addrole")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddRoleAsync(AddRoleDto model)
    {
        var result = await _userService.AddRoleAsync(model);

        if (result.IsSuccess)
            return Ok(new ApiResponse(200, "Role added successfully."));

        // Si falla, ToActionResult usará el 401 Unauthorized, 404 Not Found, 409 Conflict o 500 del service.
        return result.ToActionResult();
    }

    // 4. Refrescar Token
    [HttpPost("refresh-token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDataDto>> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        // Si el token es nulo (no está en cookie), es una petición de acceso no autorizado
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(new ApiResponse(401, "Refresh token not found in cookies."));

        var result = await _userService.RefreshTokenAsync(refreshToken);

        if (result.IsSuccess)
        {
            SetRefreshTokenInCookie(result.Data.RefreshToken);
            return Ok(new ApiResponse<UserDataDto>(200, "Token refreshed successfully.", result.Data));
        }

        // Si falla, ToActionResult usará el 401 Unauthorized, 404 Not Found o 500 del service.
        return result.ToActionResult();
    }

    // 5. Cerrar Sesión (Revocar Token)
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Logout([FromBody] LogoutDto logoutDto)
    {
        // Se mantiene la lógica de seguridad:
        // Se revoca el token (el servicio maneja el posible error 404/401 internamente),
        // pero la respuesta al cliente siempre debe ser exitosa para no revelar información de tokens.
        await _userService.RevokeRefreshTokenAsync(logoutDto.RefreshToken);

        // Limpiar la cookie (opcional, pero buena práctica)
        Response.Cookies.Delete("refreshToken");

        return Ok(new ApiResponse(200, "Logout successful."));
    }

    private void SetRefreshTokenInCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(10),
        };
        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
}