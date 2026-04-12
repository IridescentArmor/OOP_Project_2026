using Marketplace.API.DTOs;

namespace Marketplace.API.Interfaces;

public interface IAuthService
{
    AuthResponse Register(RegisterRequest request);

    AuthResponse Login(LoginRequest request);
}
