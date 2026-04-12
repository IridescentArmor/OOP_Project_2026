namespace Marketplace.API.DTOs;

public class OrderResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public string Status { get; set; } = string.Empty;

    public decimal Total { get; set; }
}
