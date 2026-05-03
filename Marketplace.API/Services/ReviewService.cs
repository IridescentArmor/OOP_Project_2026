using Marketplace.API.DTOs;
using Marketplace.API.Interfaces;
using Marketplace.API.Models;

namespace Marketplace.API.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;

    public ReviewService(
        IReviewRepository reviewRepository,
        IOrderRepository orderRepository,
        IUserRepository userRepository,
        IProductRepository productRepository)
    {
        _reviewRepository = reviewRepository;
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _productRepository = productRepository;
    }

    public IReadOnlyList<AdminReviewResponse> GetAllForAdmin()
    {
        var products = _productRepository.GetAll().ToDictionary(p => p.Id);
        var users = _userRepository.GetAll().ToDictionary(u => u.Id);

        return _reviewRepository.GetAll()
            .Select(review => new AdminReviewResponse
            {
                Id = review.Id,
                ProductId = review.ProductId,
                ProductTitle = products.TryGetValue(review.ProductId, out var product) ? product.Title : "(невідомий товар)",
                CustomerUserId = review.CustomerUserId,
                CustomerName = users.TryGetValue(review.CustomerUserId, out var user) ? user.Name : "(невідомий користувач)",
                Rating = review.Rating,
                Text = review.Text,
                IsHidden = review.IsHidden,
                ModerationReason = review.ModerationReason,
                CreatedAtUtc = review.CreatedAtUtc
            })
            .ToList();
    }

    public IReadOnlyList<ReviewResponse> GetByProduct(Guid productId, bool includeHidden)
        => _reviewRepository.GetByProduct(productId, includeHidden).Select(ToResponse).ToList();

    public ReviewResponse Create(Guid userId, Guid productId, CreateReviewRequest request)
    {
        var user = _userRepository.GetById(userId) ?? throw new InvalidOperationException("Користувача не знайдено");
        if (!user.Roles.Any(r => r.RoleName == "Customer"))
            throw new InvalidOperationException("Лише покупець може залишати відгуки");

        var hasCompletedPurchase = _orderRepository.GetAllForUser(userId)
            .Any(o =>
                o.Status == OrderStatus.Completed &&
                o.Items.Any(i => i.ProductId == productId));
        if (!hasCompletedPurchase)
            throw new InvalidOperationException("Відгук можна залишити лише на отриманий товар із завершеного замовлення");

        var review = new Review(productId, userId, request.Rating, request.Text);
        _reviewRepository.Add(review);
        return ToResponse(review);
    }

    public void Hide(Guid reviewId, string reason)
    {
        var review = _reviewRepository.GetById(reviewId) ?? throw new InvalidOperationException("Відгук не знайдено");
        review.Hide(reason);
        _reviewRepository.Update(review);
    }

    public void Unhide(Guid reviewId)
    {
        var review = _reviewRepository.GetById(reviewId) ?? throw new InvalidOperationException("Відгук не знайдено");
        review.Unhide();
        _reviewRepository.Update(review);
    }

    public void Delete(Guid reviewId, string reason)
    {
        var review = _reviewRepository.GetById(reviewId) ?? throw new InvalidOperationException("Відгук не знайдено");
        if (string.IsNullOrWhiteSpace(reason))
            throw new InvalidOperationException("Причина видалення обов'язкова");
        _reviewRepository.Delete(review);
    }

    private static ReviewResponse ToResponse(Review r) => new()
    {
        Id = r.Id,
        ProductId = r.ProductId,
        CustomerUserId = r.CustomerUserId,
        Rating = r.Rating,
        Text = r.Text,
        IsHidden = r.IsHidden,
        ModerationReason = r.ModerationReason,
        CreatedAtUtc = r.CreatedAtUtc
    };
}

