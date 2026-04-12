using Marketplace.API.Models;

namespace Marketplace.Tests.Domain;

public class CustomerRoleTests
{
    [Fact]
    public void PlaceOrder_ReducesProductStock()
    {
        var sellerId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = new Product("Тест", "Опис", 25m, 10, sellerId, categoryId);   
        var role = new CustomerRole();
        var userId = Guid.NewGuid();

        var order = role.PlaceOrder(userId, new Dictionary<Product, int> { { product, 3 } });

        Assert.Equal(7, product.StockQuantity);
        Assert.Equal(userId, order.UserId);
        Assert.Single(role.Orders);
    }

    [Fact]
    public void PlaceOrder_EmptyCart_ThrowsInvalidOperationException()
    {
        var role = new CustomerRole();

        Assert.Throws<InvalidOperationException>(() =>
            role.PlaceOrder(Guid.NewGuid(), new Dictionary<Product, int>()));
    }
}
