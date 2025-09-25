using API.Dtos.Products;
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
                return ServiceResult<RoleDto>.Failure("The role with the same name already exists.");
            }

            var role = _mapper.Map<Role>(roleDto);
            _unitOfWork.Roles.Add(role);
            await _unitOfWork.SaveAsync();

            await _unitOfWork.Products.GetByIdAsync(role.Id);

            var createdRoleDto = _mapper.Map<RoleDto>(role);
            return ServiceResult<RoleDto>.Success(createdRoleDto);
        }
        catch (Exception ex)
        {
            return ServiceResult<RoleDto>.Failure($"An error occurred while creating the role: {ex.Message}");
        }
    }

    public async Task<ServiceResult> DeleteRoleAsync(int id)
    {
        try
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
        catch (Exception ex)
        {
            return ServiceResult.Failure($"An error occurred while deleting the role: {ex.Message}");
        }
    }

    public async Task<ServiceResult<RoleDto>> GetRoleByIdAsync(int id)
    {
        try
        {
            var role = await _unitOfWork.Roles.GetByIdAsync(id);
            if(role == null)
                return ServiceResult<RoleDto>.Failure("The role requested does not exist.");

            var roleDto = _mapper.Map<RoleDto>(role);

            return ServiceResult<RoleDto>.Success(roleDto);
        }
        catch (Exception ex)
        {
            return ServiceResult<RoleDto>.Failure($"An unexpected error ocurred while retrieving the product: {ex.Message}");
        }
    }
}
