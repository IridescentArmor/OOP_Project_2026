using Marketplace.API.Data;
using Marketplace.API.DTOs;
using Marketplace.API.Models;
using Marketplace.API.Repositories;
using Marketplace.API.Services;
using Marketplace.Tests.Helpers;

namespace Marketplace.Tests.Services;

public class ProductServiceTests
{
    [Fact]
    public void DeleteAsSeller_ProductInActiveOrder_ThrowsInvalidOperationException()
    {
        var (ctx, _) = TestDatabase.CreateContext(runSeed: true);
        var productRepo = new EfProductRepository(ctx);
        var reviewRepo = new EfReviewRepository(ctx);
        var orderRepo = new EfOrderRepository(ctx);
        var userRepo = new EfUserRepository(ctx);

        var seller = User.Create("Seller", $"seller-{Guid.NewGuid():N}@svc.test", "+380671112233", "hash_pass");
        seller.AddRole(new SellerRole("ТОВ Seller"));
        userRepo.Add(seller);

        var buyer = User.Create("Buyer", $"buyer-{Guid.NewGuid():N}@svc.test", "+380501112233", "hash_pass");
        buyer.AddRole(new CustomerRole());
        userRepo.Add(buyer);

        var product = new Product("Ноутбук", "Тест", 1000m, 10, seller.Id, DbInitializer.DefaultCategoryId);
        productRepo.Add(product);

        var orderService = new OrderService(productRepo, orderRepo, userRepo);
        var createdOrder = orderService.CreateOrder(new CreateOrderRequest
        {
            UserId = buyer.Id,
            Items = new Dictionary<Guid, int> { { product.Id, 1 } },
            RecipientName = "Іван Петров",
            RecipientPhone = "+380501112233",
            ShippingAddress = "Київ, вул. Хрещатик 1"
        });

        orderService.ChangeStatus(createdOrder.Id, OrderStatus.Processing);

        var sut = new ProductService(productRepo, reviewRepo, orderRepo);

        Assert.Throws<InvalidOperationException>(() => sut.DeleteAsSeller(seller.Id, product.Id));
    }

    [Fact]
    public void DeleteAsSeller_ProductOnlyInCanceledOrder_DeletesSuccessfully()
    {
        var (ctx, _) = TestDatabase.CreateContext(runSeed: true);
        var productRepo = new EfProductRepository(ctx);
        var reviewRepo = new EfReviewRepository(ctx);
        var orderRepo = new EfOrderRepository(ctx);
        var userRepo = new EfUserRepository(ctx);

        var seller = User.Create("Seller", $"seller-{Guid.NewGuid():N}@svc.test", "+380671112233", "hash_pass");
        seller.AddRole(new SellerRole("ТОВ Seller"));
        userRepo.Add(seller);

        var buyer = User.Create("Buyer", $"buyer-{Guid.NewGuid():N}@svc.test", "+380501112233", "hash_pass");
        buyer.AddRole(new CustomerRole());
        userRepo.Add(buyer);

        var product = new Product("Смартфон", "Тест", 500m, 10, seller.Id, DbInitializer.DefaultCategoryId);
        productRepo.Add(product);

        var orderService = new OrderService(productRepo, orderRepo, userRepo);
        var createdOrder = orderService.CreateOrder(new CreateOrderRequest
        {
            UserId = buyer.Id,
            Items = new Dictionary<Guid, int> { { product.Id, 1 } },
            RecipientName = "Іван Петров",
            RecipientPhone = "+380501112233",
            ShippingAddress = "Київ, вул. Хрещатик 1"
        });

        orderService.ChangeStatus(createdOrder.Id, OrderStatus.Canceled);

        var sut = new ProductService(productRepo, reviewRepo, orderRepo);
        sut.DeleteAsSeller(seller.Id, product.Id);

        Assert.Null(productRepo.GetById(product.Id));
    }
}
