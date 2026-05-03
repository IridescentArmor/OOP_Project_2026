namespace Marketplace.API.Models;

public class Review : IEntity
{
    private Review()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ProductId { get; private set; }
    public Guid CustomerUserId { get; private set; }
    public int Rating { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public bool IsHidden { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public string? ModerationReason { get; private set; }

    public Review(Guid productId, Guid customerUserId, int rating, string text)
    {
        if (productId == Guid.Empty) throw new ArgumentException("Некоректний товар");
        if (customerUserId == Guid.Empty) throw new ArgumentException("Некоректний користувач");
        if (rating < 1 || rating > 5) throw new ArgumentException("Оцінка має бути від 1 до 5");
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Текст відгуку обов'язковий");

        ProductId = productId;
        CustomerUserId = customerUserId;
        Rating = rating;
        Text = text.Trim();
    }

    public void Hide(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Причина модерації обов'язкова");

        IsHidden = true;
        ModerationReason = reason.Trim();
    }

    public void Unhide()
    {
        IsHidden = false;
        ModerationReason = null;
    }
}

