namespace Marketplace.API.DTOs;

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public UserResponse User { get; set; } = null!;
    public Guid UserId { get; set; }
}
