namespace Marketplace.API.DTOs;

public class UserResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsBlocked { get; set; }
    public string? BlockReason { get; set; }
    public bool? IsSellerApproved { get; set; }
    public string? SellerCompanyName { get; set; }
    public int? AdminAccessLevel { get; set; }
    public DateTime RegistrationDate { get; set; }
    public IReadOnlyList<string> RoleNames { get; set; } = Array.Empty<string>();
}
