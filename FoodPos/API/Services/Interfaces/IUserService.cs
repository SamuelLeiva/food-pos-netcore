using API.Dtos.Users;
using Core.Services;

namespace API.Services.Interfaces;

public interface IUserService
{
    Task<ServiceResult> RegisterAsync(RegisterDto model);
    Task<ServiceResult<UserDataDto>> GetTokenAsync(LoginDto model);
    Task<ServiceResult> AddRoleAsync(AddRoleDto model);
    Task<ServiceResult<UserDataDto>> RefreshTokenAsync(string refreshToken);
    Task<ServiceResult> RevokeRefreshTokenAsync(string refreshToken);
}
