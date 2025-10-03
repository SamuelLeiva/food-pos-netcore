using API.Dtos.Roles;
using API.Services.Interfaces;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Core.Services;

namespace API.Services;

public class RoleService : IRoleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RoleService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ServiceResult<RoleDto>> CreateRoleAsync(RoleDto roleDto)
    {
        try
        {
            var roleExists = _unitOfWork.Roles
                .Find(r => r.Name.ToLower() == roleDto.Name.ToLower())
                .FirstOrDefault();

            if (roleExists != null)
            {
                // 409 Conflict: El recurso ya existe.
                return ServiceResult<RoleDto>.Failure("The role with the same name already exists.", 409);
            }

            var role = _mapper.Map<Role>(roleDto);
            _unitOfWork.Roles.Add(role);
            await _unitOfWork.SaveAsync();

            // Corrección: Debe ser .Roles.GetByIdAsync
            await _unitOfWork.Roles.GetByIdAsync(role.Id);

            var createdRoleDto = _mapper.Map<RoleDto>(role);
            return ServiceResult<RoleDto>.Success(createdRoleDto);
        }
        catch (Exception ex)
        {
            // 500 Internal Server Error: Para errores inesperados.
            return ServiceResult<RoleDto>.Failure($"An error occurred while creating the role: {ex.Message}", 500);
        }
    }

    public async Task<ServiceResult> DeleteRoleAsync(int id)
    {
        try
        {
            var role = await _unitOfWork.Roles.GetByIdAsync(id);
            if (role == null)
            {
                // 404 Not Found: El recurso a eliminar no existe.
                return ServiceResult.Failure("The role requested does not exist.", 404);
            }

            _unitOfWork.Roles.Remove(role);
            await _unitOfWork.SaveAsync();

            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            // 500 Internal Server Error
            return ServiceResult.Failure($"An error occurred while deleting the role: {ex.Message}", 500);
        }
    }

    public async Task<ServiceResult<RoleDto>> GetRoleByIdAsync(int id)
    {
        try
        {
            var role = await _unitOfWork.Roles.GetByIdAsync(id);
            if (role == null)
                // 404 Not Found
                return ServiceResult<RoleDto>.Failure("The role requested does not exist.", 404);

            var roleDto = _mapper.Map<RoleDto>(role);

            return ServiceResult<RoleDto>.Success(roleDto);
        }
        catch (Exception ex)
        {
            // 500 Internal Server Error
            return ServiceResult<RoleDto>.Failure($"An unexpected error ocurred while retrieving the role: {ex.Message}", 500);
        }
    }
}