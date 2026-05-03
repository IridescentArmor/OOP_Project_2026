using Marketplace.API.Models;

namespace Marketplace.Tests.Domain;

public class RoleBaseTests
{
    [Fact]
    public void DistinctKinds_ReturnsSetOfKinds()
    {
        IRole[] roles = { new CustomerRole(), new AdminRole(50), new SellerRole("ACME") };
        var kinds = RoleBase.DistinctKinds(roles);
        Assert.Equal(3, kinds.Count);
        Assert.Contains(UserRoleKind.Customer, kinds);
        Assert.Contains(UserRoleKind.Admin, kinds);
        Assert.Contains(UserRoleKind.Seller, kinds);
    }

    [Fact]
    public void UkrainianLabels_ContainsAllRoleKinds()
    {
        foreach (UserRoleKind k in Enum.GetValues<UserRoleKind>())
            Assert.True(RoleBase.UkrainianLabels.ContainsKey(k));
    }
}
