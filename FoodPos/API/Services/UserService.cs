using API.Dtos.Users;
using API.Helpers;
using API.Services.Interfaces;
using Core.Entities;
using Core.Interfaces;
using Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace API.Services;

public class UserService : IUserService
{
    private readonly JWT _jwt;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserService(IUnitOfWork unitOfWork, IOptions<JWT> jwt,
        IPasswordHasher<User> passwordHasher)
    {
        _jwt = jwt.Value;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<ServiceResult> RegisterAsync(RegisterDto registerDto)
    {
        try
        {
            var userExists = _unitOfWork.Users
                                    .Find(u => u.UserName.ToLower() == registerDto.UserName.ToLower() || u.Email.ToLower() == registerDto.Email.ToLower())
                                    .FirstOrDefault();
            if (userExists != null)
                return ServiceResult.Failure($"The email or username is already registered.");

            var user = new User
            {
                Names = registerDto.Names,
                FirstSurname = registerDto.FirstSurname,
                LastSurname = registerDto.LastSurname,
                Email = registerDto.Email,
                UserName = registerDto.UserName
            };

            // encriptación de contraseña
            user.Password = _passwordHasher.HashPassword(user, registerDto.Password);

            var defaultRole = _unitOfWork.Roles
                                    .Find(u => u.Name == Authorization.default_role.ToString())
                                    .FirstOrDefault();

            if (defaultRole == null)
                return ServiceResult.Failure("Default role not found. Please contact support.");

            user.Roles.Add(defaultRole);
            _unitOfWork.Users.Add(user);
            await _unitOfWork.SaveAsync();

            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            return ServiceResult.Failure($"An unexpected error occurred during registration: {ex.Message}");
        }
    }

    public async Task<ServiceResult<UserDataDto>> GetTokenAsync(LoginDto model)
    {
        try
        {
            var user = await _unitOfWork.Users
                                .GetByUserNameAsync(model.UserName);

            if (user == null)
                return ServiceResult<UserDataDto>.Failure($"There is no user with the username {model.UserName}.");

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, model.Password);

            if (result != PasswordVerificationResult.Success)
                return ServiceResult<UserDataDto>.Failure($"Wrong credentials for user {model.UserName}.");

            var userDataDto = new UserDataDto();
            userDataDto.IsAuth = true;
            JwtSecurityToken jwtSecurityToken = CreateJwtToken(user);
            userDataDto.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            userDataDto.Email = user.Email;
            userDataDto.UserName = user.UserName;
            userDataDto.Roles = user.Roles.Select(u => u.Name).ToList();

            var activeRefreshToken = user.RefreshTokens.FirstOrDefault(a => a.IsActive);
            if (activeRefreshToken != null)
            {
                userDataDto.RefreshToken = activeRefreshToken.Token;
                userDataDto.RefreshTokenExpiration = activeRefreshToken.Expires;
            }
            else
            {
                var newRefreshToken = CreateRefreshToken();
                userDataDto.RefreshToken = newRefreshToken.Token;
                userDataDto.RefreshTokenExpiration = newRefreshToken.Expires;
                user.RefreshTokens.Add(newRefreshToken);
                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveAsync();
            }

            return ServiceResult<UserDataDto>.Success(userDataDto);
        }
        catch (Exception ex)
        {
            return ServiceResult<UserDataDto>.Failure($"An unexpected error occurred during login: {ex.Message}");
        }
    }

    public async Task<ServiceResult<UserDataDto>> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByRefreshTokenAsync(refreshToken);

            if (user == null)
                return ServiceResult<UserDataDto>.Failure($"The token does not belong to any user.");

            var refreshTokenBd = user.RefreshTokens.Single(x => x.Token == refreshToken);

            if (!refreshTokenBd.IsActive)
                return ServiceResult<UserDataDto>.Failure($"The token is not active.");

            refreshTokenBd.Revoked = DateTime.UtcNow;
            var newRefreshToken = CreateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveAsync();

            var userDataDto = new UserDataDto();
            userDataDto.IsAuth = true;
            JwtSecurityToken jwtSecurityToken = CreateJwtToken(user);
            userDataDto.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            userDataDto.Email = user.Email;
            userDataDto.UserName = user.UserName;
            userDataDto.Roles = user.Roles.Select(u => u.Name).ToList();
            userDataDto.RefreshToken = newRefreshToken.Token;
            userDataDto.RefreshTokenExpiration = newRefreshToken.Expires;

            return ServiceResult<UserDataDto>.Success(userDataDto);
        }
        catch (Exception ex)
        {
            return ServiceResult<UserDataDto>.Failure($"An unexpected error occurred during token refresh: {ex.Message}");
        }
    }

    public async Task<ServiceResult> RevokeRefreshTokenAsync(string refreshToken)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByRefreshTokenAsync(refreshToken);

            // Returning success even if the token is not found is a good security practice
            // to prevent leaking information about valid/invalid tokens.
            if (user == null)
                return ServiceResult.Success();

            var refreshTokenEntity = user.RefreshTokens.SingleOrDefault(t => t.Token == refreshToken);

            if (refreshTokenEntity == null || refreshTokenEntity.Revoked.HasValue)
                return ServiceResult.Success();

            refreshTokenEntity.Revoked = DateTime.UtcNow;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveAsync();

            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            return ServiceResult.Failure($"An unexpected error occurred while revoking the token: {ex.Message}");
        }
    }


    private RefreshToken CreateRefreshToken()
    {
        var randomNumber = new byte[32];
        // se suele usar el ip del usuario en casos reales
        using (var generator = RandomNumberGenerator.Create())
        {
            generator.GetBytes(randomNumber);
            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                Expires = DateTime.UtcNow.AddDays(10),
                Created = DateTime.UtcNow
            };
        }
    }


    private JwtSecurityToken CreateJwtToken(User user)
    {
        var roles = user.Roles;
        var roleClaims = new List<Claim>();
        foreach (var role in roles)
        {
            roleClaims.Add(new Claim(ClaimTypes.Role, role.Name));
        }
        var claims = new[]
        {
                                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                                new Claim("uid", user.Id.ToString())
                        }
        .Union(roleClaims);
        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
        var jwtSecurityToken = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.DurationInMinutes),
            signingCredentials: signingCredentials);
        return jwtSecurityToken;
    }

    public async Task<ServiceResult> AddRoleAsync(AddRoleDto model)
    {

        try
        {
            var user = await _unitOfWork.Users.GetByUserNameAsync(model.UserName);
            if (user == null)
                return ServiceResult.Failure($"There is no user with the username {model.UserName}.");

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, model.Password);
            if (result != PasswordVerificationResult.Success)
                return ServiceResult.Failure($"Wrong credentials for user {model.UserName}.");

            var rolExists = _unitOfWork.Roles.Find(u => u.Name.ToLower() == model.Role.ToLower()).FirstOrDefault();
            if (rolExists == null)
                return ServiceResult.Failure($"{model.Role} role not found.");

            var userHasRole = user.Roles.Any(u => u.Id == rolExists.Id);
            if (userHasRole)
                return ServiceResult.Failure($"User {model.UserName} already has the role {model.Role}.");

            user.Roles.Add(rolExists);
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveAsync();

            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            return ServiceResult.Failure($"An unexpected error occurred while adding the role: {ex.Message}");
        }
    }

}

