using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Marketplace.API.DTOs;
using Marketplace.API.Interfaces;
using Marketplace.API.Models;
using Marketplace.API.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Marketplace.API.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly JwtOptions _jwt;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher<User> passwordHasher,
        IOptions<JwtOptions> jwtOptions)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwt = jwtOptions.Value;
    }

    public AuthResponse Register(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (_userRepository.GetByEmail(email) != null)
            throw new InvalidOperationException("Користувач з таким email вже існує");

        var user = User.Create(
            request.Name.Trim(),
            email,
            request.PhoneNumber.Trim(),
            "temp");

        var hash = _passwordHasher.HashPassword(user, request.Password);
        user.ChangePasswordHash(hash);

        if (request.IncludeCustomerRole)
            user.AddRole(new CustomerRole());

        if (request.IncludeSellerRole)
            user.AddRole(new SellerRole(request.SellerCompanyName!.Trim()));

        if (request.IncludeAdminRole)
            user.AddRole(new AdminRole(request.AdminAccessLevel));

        _userRepository.Add(user);
        return BuildAuthResponse(user);
    }

    public AuthResponse Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = _userRepository.GetByEmail(email)
            ?? throw new InvalidOperationException("Невірний email або пароль");

        var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verify == PasswordVerificationResult.Failed)
            throw new InvalidOperationException("Невірний email або пароль");

        return BuildAuthResponse(user);
    }

    private AuthResponse BuildAuthResponse(User user)
    {
        var expires = DateTime.UtcNow.AddMinutes(_jwt.ExpireMinutes);
        var token = CreateJwt(user, expires);

        return new AuthResponse
        {
            AccessToken = token,
            ExpiresAtUtc = expires,
            User = ToUserResponse(user)
        };
    }

    private string CreateJwt(User user, DateTime expiresUtc)
    {
        if (string.IsNullOrWhiteSpace(_jwt.SigningKey) || _jwt.SigningKey.Length < 32)
            throw new InvalidOperationException("JWT SigningKey має містити щонайменше 32 символи");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Name, user.Name)
        };

        foreach (var role in user.Roles)
            claims.Add(new Claim(ClaimTypes.Role, role.RoleName));

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expiresUtc,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserResponse ToUserResponse(User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        PhoneNumber = user.PhoneNumber,
        RoleNames = user.Roles.Select(r => r.RoleName).ToList()
    };
}
