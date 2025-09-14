using API.Dtos.Roles;
using Core.Entities;
using Core.Services;

namespace API.Services.Interfaces;

public interface IRoleService
{
    Task<ServiceResult<Role>> CreateRoleAsync(CreateRoleDto roleDto);
    Task<ServiceResult> DeleteRoleAsync(int id);
}
