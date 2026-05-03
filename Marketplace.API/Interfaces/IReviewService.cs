using Marketplace.API.DTOs;

namespace Marketplace.API.Interfaces;

public interface IReviewService
{
    IReadOnlyList<AdminReviewResponse> GetAllForAdmin();
    IReadOnlyList<ReviewResponse> GetByProduct(Guid productId, bool includeHidden);
    ReviewResponse Create(Guid userId, Guid productId, CreateReviewRequest request);
    void Hide(Guid reviewId, string reason);
    void Unhide(Guid reviewId);
    void Delete(Guid reviewId, string reason);
}

