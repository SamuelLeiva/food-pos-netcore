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

    public async Task<ServiceResult<Role>> CreateRoleAsync(CreateRoleDto roleDto)
    {
        var roleExists = _unitOfWork.Roles
            .Find(r => r.Name.ToLower() == roleDto.Name.ToLower())
            .FirstOrDefault();

        if (roleExists != null)
        {
            return ServiceResult<Role>.Failure("The role with the same name already exists.");
        }

        var role = _mapper.Map<Role>(roleDto);

        _unitOfWork.Roles.Add(role);
        await _unitOfWork.SaveAsync();

        if (role == null)
        {
            return ServiceResult<Role>.Failure("Failed to create role.");
        }

        return ServiceResult<Role>.Success(role);
    }

    public async Task<ServiceResult> DeleteRoleAsync(int id)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(id);
        if (role == null)
        {
            return ServiceResult.Failure("The role requested does not exist.");
        }

        _unitOfWork.Roles.Remove(role);
        await _unitOfWork.SaveAsync();

        return ServiceResult.Success();
    }
}
