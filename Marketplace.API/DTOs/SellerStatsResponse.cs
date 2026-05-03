namespace Marketplace.API.DTOs;

public class SellerStatsResponse
{
    public int PeriodDays { get; set; }
    public int CompletedOrdersCount { get; set; }
    public decimal Revenue { get; set; }
    public int SoldItemsCount { get; set; }
    public IReadOnlyList<SellerStatsPointResponse> Points { get; set; } = Array.Empty<SellerStatsPointResponse>();
}

