using API.Dtos.Users;
using API.Helpers.Errors;
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
        if (!result.IsSuccess)
        {
            return Conflict(new ApiResponse(409, result.ErrorMessage));
        }

        return new CreatedResult(string.Empty, new ApiResponse(201, "User registered successfully."));
    }

    [HttpPost("token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTokenAsync(LoginDto model)
    {
        var result = await _userService.GetTokenAsync(model);
        if (!result.IsSuccess)
        {
            return BadRequest(new ApiResponse(400, result.ErrorMessage));
        }
        SetRefreshTokenInCookie(result.Data.RefreshToken);
        return Ok(result.Data);
    }

    //[Authorize(Roles = "Admin")]
    [HttpPost("addrole")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddRoleAsync(AddRoleDto model)
    {
        var result = await _userService.AddRoleAsync(model);
        if (!result.IsSuccess)
        {
            return BadRequest(new ApiResponse(400, result.ErrorMessage));
        }
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        var result = await _userService.RefreshTokenAsync(refreshToken);
        if (!string.IsNullOrEmpty(result.Data.RefreshToken))
            SetRefreshTokenInCookie(result.Data.RefreshToken);
        return Ok(result.Data);
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Logout(LogoutDto logoutDto)
    {
        // Retrieve the refresh token from the request body.
        var result = await _userService.RevokeRefreshTokenAsync(logoutDto.RefreshToken);

        // Even if the token is already revoked or not found, we return a success status code
        // to avoid leaking information about why the logout failed.
        return Ok(new ApiResponse(200, "Logout successful"));
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
