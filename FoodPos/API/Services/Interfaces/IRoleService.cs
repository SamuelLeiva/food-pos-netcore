using API.Dtos.Roles;
using Core.Entities;
using Core.Services;

namespace API.Services.Interfaces;

public interface IRoleService
{
    Task<ServiceResult<Role>> CreateRoleAsync(RoleDto roleDto);
    Task<ServiceResult> DeleteRoleAsync(int id);
    Task<ServiceResult<RoleDto>> GetRoleByIdAsync(int id);
}
