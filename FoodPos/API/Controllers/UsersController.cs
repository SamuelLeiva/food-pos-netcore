using API.Dtos.Users;
using API.Helpers.Errors;
using API.Helpers.Response;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class UsersController : BaseApiController
{
    private readonly IUserService _userService;
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> RegisterAsync(RegisterDto model)
    {
        var result = await _userService.RegisterAsync(model);
        if (result.IsSuccess)
            return new CreatedResult(string.Empty, new ApiResponse(201, "User registered successfully."));

        return Conflict(new ApiResponse(409, result.ErrorMessage));
    }

    [HttpPost("token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDataDto>> GetTokenAsync(LoginDto model)
    {
        var result = await _userService.GetTokenAsync(model);
        if (result.IsSuccess)
        {
            SetRefreshTokenInCookie(result.Data.RefreshToken);
            return Ok(new ApiResponse<UserDataDto>(200, "Token generated successfully.", result.Data));
        }
        return BadRequest(new ApiResponse(400, result.ErrorMessage));
    }

    //[Authorize(Roles = "Admin")]
    [HttpPost("addrole")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddRoleAsync(AddRoleDto model)
    {
        var result = await _userService.AddRoleAsync(model);
        if (result.IsSuccess)
            return Ok(new ApiResponse(200, "Role added successfully."));
        // Podemos ser más específicos si el error es de tipo "not found"

        if (result.ErrorMessage.Contains("not found"))
            return NotFound(new ApiResponse(404, result.ErrorMessage));

        return BadRequest(new ApiResponse(400, result.ErrorMessage));
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDataDto>> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        var result = await _userService.RefreshTokenAsync(refreshToken);

        if (result.IsSuccess)
        {
            SetRefreshTokenInCookie(result.Data.RefreshToken);
            return Ok(new ApiResponse<UserDataDto>(200, "Token refreshed successfully.", result.Data));
        }

        return BadRequest(new ApiResponse(400, result.ErrorMessage));
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Logout([FromBody] LogoutDto logoutDto)
    {
        var result = await _userService.RevokeRefreshTokenAsync(logoutDto.RefreshToken);

        // No importa si el resultado es éxito o fallo, el logout es exitoso desde la perspectiva del cliente.
        // Esto previene que un atacante descubra si un token es válido o no.
        if (result.IsSuccess)
        {
            return Ok(new ApiResponse(200, "Logout successful."));
        }

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
