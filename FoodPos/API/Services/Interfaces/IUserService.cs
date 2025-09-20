using API.Dtos.Users;
using Core.Services;

namespace API.Services.Interfaces;

public interface IUserService
{
    Task<ServiceResult> RegisterAsync(RegisterDto model);
    Task<UserDataDto> GetTokenAsync(LoginDto model);
    Task<string> AddRoleAsync(AddRoleDto model);
    Task<UserDataDto> RefreshTokenAsync(string refreshToken);
}
