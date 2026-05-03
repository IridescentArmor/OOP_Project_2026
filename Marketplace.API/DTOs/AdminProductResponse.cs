namespace Marketplace.API.DTOs;

public class AdminProductResponse
{
    public ProductResponse Product { get; set; } = new();
    public string SellerName { get; set; } = string.Empty;
}
