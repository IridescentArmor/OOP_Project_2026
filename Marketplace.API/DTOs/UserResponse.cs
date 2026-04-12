namespace Marketplace.API.DTOs;

public class UserResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public IReadOnlyList<string> RoleNames { get; set; } = Array.Empty<string>();
}
