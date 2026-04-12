namespace Marketplace.API.Data;

public class UserRoleRow
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string RoleType { get; set; } = string.Empty;

    public string? CustomerDefaultAddress { get; set; }
    public int CustomerLoyaltyPoints { get; set; }

    public string? SellerCompanyName { get; set; }
    public double SellerRating { get; set; }

    public int? AdminAccessLevel { get; set; }
}
