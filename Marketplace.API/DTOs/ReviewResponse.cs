namespace Marketplace.API.DTOs;

public class ReviewResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid CustomerUserId { get; set; }
    public int Rating { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsHidden { get; set; }
    public string? ModerationReason { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

