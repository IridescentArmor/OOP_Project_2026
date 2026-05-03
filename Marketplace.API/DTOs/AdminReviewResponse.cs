namespace Marketplace.API.DTOs;

public class AdminReviewResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductTitle { get; set; } = string.Empty;
    public Guid CustomerUserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsHidden { get; set; }
    public string? ModerationReason { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
