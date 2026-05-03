namespace Marketplace.API.DTOs;

public class SellerStatsPointResponse
{
    public string Label { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int OrdersCount { get; set; }
}
