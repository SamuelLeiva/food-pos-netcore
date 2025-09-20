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
        var usuario = new User
        {
            Names = registerDto.Names,
            FirstSurname = registerDto.FirstSurname,
            LastSurname = registerDto.LastSurname,
            Email = registerDto.Email,
            UserName = registerDto.UserName
        };

        // encriptación de contraseña
        usuario.Password = _passwordHasher.HashPassword(usuario, registerDto.Password);

        var userExists = _unitOfWork.Users
                                    .Find(u => u.UserName.ToLower() == registerDto.UserName.ToLower() || u.Email.ToLower() == registerDto.Email.ToLower())
                                    .FirstOrDefault();

        if (userExists == null)
        {
            var defaultRole = _unitOfWork.Roles
                                    .Find(u => u.Name == Authorization.default_role.ToString())
                                    .First();

                usuario.Roles.Add(defaultRole);
                _unitOfWork.Users.Add(usuario);
                await _unitOfWork.SaveAsync();

                return ServiceResult.Success();
        }
        else
        {
            return ServiceResult.Failure($"The email {registerDto.Email} is already registered.");
        }
    }

    public async Task<ServiceResult<UserDataDto>> GetTokenAsync(LoginDto model)
    {
        UserDataDto userDataDto = new UserDataDto();
        var user = await _unitOfWork.Users
                    .GetByUserNameAsync(model.UserName);

        if (user == null)
        {
            userDataDto.IsAuth = false;
            userDataDto.Message = $"There is no user with the username {model.UserName}.";
            return ServiceResult<UserDataDto>.Failure(userDataDto.Message);
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.Password, model.Password);

        if (result == PasswordVerificationResult.Success)
        {
            userDataDto.IsAuth = true;
            JwtSecurityToken jwtSecurityToken = CreateJwtToken(user);
            userDataDto.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            userDataDto.Email = user.Email;
            userDataDto.UserName = user.UserName;
            userDataDto.Roles = user.Roles
                                            .Select(u => u.Name)
                                            .ToList();

            if (user.RefreshTokens.Any(a => a.IsActive))
            {
                var activeRefreshToken = user.RefreshTokens.Where(a => a.IsActive == true).FirstOrDefault();
                userDataDto.RefreshToken = activeRefreshToken.Token;
                userDataDto.RefreshTokenExpiration = activeRefreshToken.Expires;
            }
            else
            {
                var refreshToken = CreateRefreshToken();
                userDataDto.RefreshToken = refreshToken.Token;
                userDataDto.RefreshTokenExpiration = refreshToken.Expires;
                user.RefreshTokens.Add(refreshToken);
                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveAsync();
            }

            return ServiceResult<UserDataDto>.Success(userDataDto);
        }
        userDataDto.IsAuth = false;
        userDataDto.Message = $"Wrong credentials of user {user.UserName}.";
        return ServiceResult<UserDataDto>.Failure(userDataDto.Message);
    }

    public async Task<ServiceResult<UserDataDto>> RefreshTokenAsync(string refreshToken)
    {
        var userDataDto = new UserDataDto();

        var user = await _unitOfWork.Users
                        .GetByRefreshTokenAsync(refreshToken);

        if (user == null)
        {
            userDataDto.IsAuth = false;
            userDataDto.Message = $"The token does not belong to any user.";
            return ServiceResult<UserDataDto>.Failure(userDataDto.Message);
        }

        var refreshTokenBd = user.RefreshTokens.Single(x => x.Token == refreshToken);

        if (!refreshTokenBd.IsActive)
        {
            userDataDto.IsAuth = false;
            userDataDto.Message = $"The token is not active.";
            return ServiceResult<UserDataDto>.Failure(userDataDto.Message);
        }
        //Revocamos el Refresh Token actual y
        refreshTokenBd.Revoked = DateTime.UtcNow;
        //generamos un nuevo Refresh Token y lo guardamos en la Base de Datos
        var newRefreshToken = CreateRefreshToken();
        user.RefreshTokens.Add(newRefreshToken);
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveAsync();
        //Generamos un nuevo Json Web Token
        userDataDto.IsAuth = true;
        JwtSecurityToken jwtSecurityToken = CreateJwtToken(user);
        userDataDto.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        userDataDto.Email = user.Email;
        userDataDto.UserName = user.UserName;
        userDataDto.Roles = user.Roles
                                        .Select(u => u.Name)
                                        .ToList();
        userDataDto.RefreshToken = newRefreshToken.Token;
        userDataDto.RefreshTokenExpiration = newRefreshToken.Expires;
        return ServiceResult<UserDataDto>.Success(userDataDto);
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
            roleClaims.Add(new Claim("roles", role.Name));
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

        var user = await _unitOfWork.Users
                    .GetByUserNameAsync(model.UserName);

        if (user == null)
        {
            return ServiceResult.Failure($"There is no user with account {model.UserName}.");
        }


        var result = _passwordHasher.VerifyHashedPassword(user, user.Password, model.Password);

        if (result == PasswordVerificationResult.Success)
        {


            var rolExists = _unitOfWork.Roles
                                        .Find(u => u.Name.ToLower() == model.Role.ToLower())
                                        .FirstOrDefault();

            if (rolExists != null)
            {
                var userHasRole = user.Roles
                                            .Any(u => u.Id == rolExists.Id);

                if (userHasRole == false)
                {
                    user.Roles.Add(rolExists);
                    _unitOfWork.Users.Update(user);
                    await _unitOfWork.SaveAsync();
                }

                return ServiceResult.Success();
            }

            return ServiceResult.Failure($"{model.Role} role not found.");
        }
        return ServiceResult.Failure($"Wrong credentials of user {user.UserName}.");
    }

}

