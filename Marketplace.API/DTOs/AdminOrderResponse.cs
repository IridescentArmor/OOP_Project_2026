namespace Marketplace.API.DTOs;

public class AdminOrderResponse
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string SellerName { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string? TrackingNumber { get; set; }
}
