using API.Dtos.Roles;
using API.Helpers.Errors;
using API.Helpers.Response;
using API.Services.Interfaces;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.UnitOfWork;
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
    public async Task<ActionResult<RoleDto>> Post(RoleDto roleDto)
    {
        var result = await _roleService.CreateRoleAsync(roleDto);
        if (result.IsSuccess)
            return CreatedAtAction(nameof(Get), new { id = result.Data.Id }, new ApiResponse<RoleDto>(201, "Role created successfully.", result.Data));

        return Conflict(new ApiResponse(409, result.ErrorMessage));

    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _roleService.DeleteRoleAsync(id);
        if (result.IsSuccess)
            return NoContent();

        return NotFound(new ApiResponse(404, result.ErrorMessage));


    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleDto>> Get(int id)
    {
        var result = await _roleService.GetRoleByIdAsync(id);
        if (result.IsSuccess)
            return Ok(new ApiResponse<RoleDto>(200, "Role retrieved successfully.", result.Data));

        return NotFound(new ApiResponse(404, result.ErrorMessage));
    }
}
