using Marketplace.API.Models;

namespace Marketplace.Tests.Domain;

public class UserRoleTests
{
    [Fact]
    public void AddRole_And_HasRole_Customer_ReturnsTrue()
    {
        var user = User.Create("testssss", "test@local", "+380501112233", "passasword");
        user.AddRole(new CustomerRole());

        Assert.True(user.HasRole<CustomerRole>());
        Assert.False(user.HasRole<SellerRole>());
    }

    [Fact]
    public void AddRole_Null_ThrowsArgumentNullException()
    {
        var user = User.Create("testsssssssss", "test@local", "+380501112233", "passasasasword");

        Assert.Throws<ArgumentNullException>(() => user.AddRole(null!));
    }
}
