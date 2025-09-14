using API.Dtos;
using API.Dtos.Roles;
using API.Helpers.Errors;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize(Roles = "Admin")]
public class RolesController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RolesController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Role>> Post(RoleDto roleDto)
    {
        var roleExists = _unitOfWork.Roles
                                    .Find(r => r.Name.ToLower() == roleDto.Name.ToLower())
                                    .FirstOrDefault();
        if (roleExists != null)
            return Conflict(new ApiResponse(409, "The role with the same name already exists."));

        var role = _mapper.Map<Role>(roleDto);

        _unitOfWork.Roles.Add(role);
        await _unitOfWork.SaveAsync();

        if (role == null)
        {
            return BadRequest(new ApiResponse(400));
        }

        return CreatedAtAction(nameof(Post), roleDto);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(id);
        if (role == null)
            return NotFound(new ApiResponse(404, "The role requested does not exist."));

        _unitOfWork.Roles.Remove(role);
        await _unitOfWork.SaveAsync();

        return NoContent();
    }
}
