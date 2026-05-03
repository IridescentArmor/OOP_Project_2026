namespace Marketplace.API.DTOs;

public class OrderResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid SellerId { get; set; }

    public string Status { get; set; } = string.Empty;

    public decimal Total { get; set; }

    public string? TrackingNumber { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;

    public IReadOnlyList<OrderItemResponse> Items { get; set; } = Array.Empty<OrderItemResponse>();
}
