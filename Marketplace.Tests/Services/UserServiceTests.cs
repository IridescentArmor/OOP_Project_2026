using Marketplace.API.DTOs;
using Marketplace.API.Models;
using Marketplace.API.Repositories;
using Marketplace.API.Services;
using Marketplace.Tests.Helpers;
using Microsoft.AspNetCore.Identity;

namespace Marketplace.Tests.Services;

public class UserServiceTests
{
    [Fact]
    public void Create_DuplicateEmail_ThrowsInvalidOperationException()
    {
        var (ctx, _) = TestDatabase.CreateContext(runSeed: false);
        var repo = new EfUserRepository(ctx);
        var sut = new UserService(repo, new EfOrderRepository(ctx), new PasswordHasher<User>());

        var req = new CreateUserRequest
        {
            Name = "Перший",
            Email = "same@svc.test",
            PhoneNumber = "+380501112233",
            PasswordHash = "hash_pass",
            IncludeCustomerRole = true
        };

        sut.Create(req);

        var duplicate = new CreateUserRequest
        {
            Name = "Другий",
            Email = "same@svc.test",
            PhoneNumber = "+380501112233",
            PasswordHash = "hash_pass",
            IncludeCustomerRole = true
        };

        Assert.Throws<InvalidOperationException>(() => sut.Create(duplicate));
    }

    [Fact]
    public void Create_WithSellerRole_AddsCompany()
    {
        var (ctx, _) = TestDatabase.CreateContext(runSeed: false);
        var repo = new EfUserRepository(ctx);
        var sut = new UserService(repo, new EfOrderRepository(ctx), new PasswordHasher<User>());

        var res = sut.Create(new CreateUserRequest
        {
            Name = "Продавець",
            Email = "seller@svc.test",
            PhoneNumber = "+380501112233",
            PasswordHash = "h",
            IncludeSellerRole = true,
            SellerCompanyName = "ТОВ Тест"
        });

        Assert.Contains("Seller", res.RoleNames);
    }
}
