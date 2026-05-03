using Marketplace.API.Data;
using Marketplace.API.DTOs;
using Marketplace.API.Models;
using Marketplace.API.Repositories;
using Marketplace.API.Services;
using Marketplace.Tests.Helpers;

namespace Marketplace.Tests.Services;

public class OrderServiceTests
{
    [Fact]
    public void CreateOrder_ValidUserAndStock_ReturnsOrderResponse()
    {
        var (ctx, _) = TestDatabase.CreateContext(runSeed: true);
        var productRepo = new EfProductRepository(ctx);
        var orderRepo = new EfOrderRepository(ctx);
        var userRepo = new EfUserRepository(ctx);

        var user = userRepo.GetById(DbInitializer.DemoBuyerUserId)
            ?? throw new InvalidOperationException("Демо-користувач не знайдений після сиду");

        var product = productRepo.GetAll().First();
        var beforeStock = product.StockQuantity;

        var sut = new OrderService(productRepo, orderRepo, userRepo);
        var result = sut.CreateOrder(new CreateOrderRequest
        {
            UserId = user.Id,
            Items = new Dictionary<Guid, int> { { product.Id, 1 } },
            RecipientName = "Іван Петров",
            RecipientPhone = "+380501112233",
            ShippingAddress = "Київ, вул. Хрещатик 1"
        });

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(beforeStock - 1, product.StockQuantity);
    }

    [Fact]
    public void CreateOrder_UserWithoutCustomerRole_ThrowsInvalidOperationException()
    {
        var (ctx, _) = TestDatabase.CreateContext(runSeed: true);
        var productRepo = new EfProductRepository(ctx);
        var orderRepo = new EfOrderRepository(ctx);
        var userRepo = new EfUserRepository(ctx);

        var user = User.Create("Без ролі", "norole@svc.test", "+380671112233", "hasfafsfash");
        userRepo.Add(user);

        var product = productRepo.GetAll().First();
        var sut = new OrderService(productRepo, orderRepo, userRepo);

        Assert.Throws<InvalidOperationException>(() => sut.CreateOrder(new CreateOrderRequest
        {
            UserId = user.Id,
            Items = new Dictionary<Guid, int> { { product.Id, 1 } },
            RecipientName = "Іван Петров",
            RecipientPhone = "+380501112233",
            ShippingAddress = "Київ, вул. Хрещатик 1"
        }));
    }
}
