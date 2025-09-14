using API.Dtos.Roles;
using API.Helpers.Errors;
using API.Services.Interfaces;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize(Roles = "Admin")]
public class RolesController : BaseApiController
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Role>> Post(CreateRoleDto roleDto)
    {
        var result = await _roleService.CreateRoleAsync(roleDto);
        if (!result.IsSuccess)
        {
            return Conflict(new ApiResponse(409, result.ErrorMessage));
        }

        return CreatedAtAction(nameof(Post), result.Data);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _roleService.DeleteRoleAsync(id);
        if (!result.IsSuccess)
        {
            return NotFound(new ApiResponse(404, result.ErrorMessage));
        }

        return NoContent();
    }
}
