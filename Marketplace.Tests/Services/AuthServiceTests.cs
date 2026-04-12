using Marketplace.API.DTOs;
using Marketplace.API.Options;
using Marketplace.API.Repositories;
using Marketplace.API.Services;
using Marketplace.Tests.Helpers;
using Microsoft.AspNetCore.Identity;
using Marketplace.API.Models;
using Microsoft.Extensions.Options;

namespace Marketplace.Tests.Services;

public class AuthServiceTests
{
    [Fact]
    public void Register_Then_Login_ReturnsToken()
    {
        var (ctx, _) = TestDatabase.CreateContext(runSeed: false);
        var hasher = new PasswordHasher<User>();
        var userRepo = new EfUserRepository(ctx);
        var jwt = Options.Create(new JwtOptions
        {
            Issuer = "Test",
            Audience = "Test",
            SigningKey = "UnitTest_Signing_Key_MinLength32Chars!!",
            ExpireMinutes = 60
        });

        var sut = new AuthService(userRepo, hasher, jwt);

        sut.Register(new RegisterRequest
        {
            Name = "Auth User",
            Email = "auth@svc.test",
            PhoneNumber = "+380501112233",
            Password = "password",
            IncludeCustomerRole = true
        });

        var auth = sut.Login(new LoginRequest
        {
            Email = "auth@svc.test",
            Password = "password"
        });

        Assert.False(string.IsNullOrWhiteSpace(auth.AccessToken));
        Assert.Equal("auth@svc.test", auth.User.Email);
        Assert.Contains("Customer", auth.User.RoleNames);
    }
}
