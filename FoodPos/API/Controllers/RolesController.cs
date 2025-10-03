using API.Dtos.Roles;
using API.Helpers.Response;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.Extensions; // Necesario para usar ToActionResult()

namespace API.Controllers;

[Authorize(Roles = "Admin")]
public class RolesController : BaseApiController
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    // 1. Crear un nuevo rol
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RoleDto>> Post(RoleDto roleDto)
    {
        var result = await _roleService.CreateRoleAsync(roleDto);

        if (result.IsSuccess)
            // Usamos CreatedAtAction para el 201 RESTful
            return CreatedAtAction(nameof(Get), new { id = result.Data.Id }, new ApiResponse<RoleDto>(201, "Role created successfully.", result.Data));

        // Si falla, ToActionResult usará el 409 Conflict o el 500 del service.
        return result.ToActionResult();
    }

    // 2. Eliminar un rol
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _roleService.DeleteRoleAsync(id);

        if (result.IsSuccess)
            // Retorna 204 No Content para eliminación exitosa
            return NoContent();

        // Si falla, ToActionResult usará el 404 Not Found (desde el service)
        return result.ToActionResult();
    }

    // 3. Obtener rol por ID
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RoleDto>> Get(int id)
    {
        var result = await _roleService.GetRoleByIdAsync(id);

        // Si falla, ToActionResult usará el 404 Not Found o el 500 del service.
        return result.ToActionResult();
    }
}