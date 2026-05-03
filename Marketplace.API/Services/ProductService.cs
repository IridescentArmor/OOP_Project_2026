using Marketplace.API.DTOs;
using Marketplace.API.Interfaces;
using Marketplace.API.Models;

namespace Marketplace.API.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IReviewRepository _reviewRepository;

    public ProductService(
        IProductRepository productRepository,
        IReviewRepository reviewRepository)
    {
        _productRepository = productRepository;
        _reviewRepository = reviewRepository;
    }

    public IEnumerable<ProductResponse> GetAll()
        => _productRepository.GetAllVisibleForGuests().Select(ToResponse);

    public IEnumerable<ProductResponse> GetAllForAdmin()
        => _productRepository.GetAll().Select(ToResponse);

    public ProductResponse? GetById(Guid id)
    {
        var product = _productRepository.GetById(id);
        return product == null ? null : ToResponse(product);
    }

    public ProductResponse Create(CreateProductRequest request)
    {
        var product = new Product(
            request.Title,
            request.Description,
            request.Price,
            request.StockQuantity,
            request.SellerId,
            request.CategoryId);
        product.SetImageUrl(request.ImageUrl);

        _productRepository.Add(product);
        return ToResponse(product);
    }

    public ProductResponse CreateAsSeller(Guid sellerUserId, CreateProductRequest request)
    {
        var product = new Product(
            request.Title,
            request.Description,
            request.Price,
            request.StockQuantity,
            sellerUserId,
            request.CategoryId);
        product.SetImageUrl(request.ImageUrl);

        _productRepository.Add(product);
        return ToResponse(product);
    }

    public ProductResponse UpdateAsSeller(Guid sellerUserId, Guid productId, CreateProductRequest request)
    {
        var product = _productRepository.GetById(productId)
            ?? throw new InvalidOperationException("Товар не знайдено");

        if (product.SellerId != sellerUserId)
            throw new InvalidOperationException("Це не ваш товар");

        product.ChangeTitle(request.Title);
        product.ChangeDescription(request.Description);
        product.SetPrice(request.Price);
        product.SetStockQuantity(request.StockQuantity);
        product.ChangeCategory(request.CategoryId);
        product.SetImageUrl(request.ImageUrl);

        _productRepository.Update(product);
        return ToResponse(product);
    }

    public void DeleteAsSeller(Guid sellerUserId, Guid productId)
    {
        var product = _productRepository.GetById(productId)
            ?? throw new InvalidOperationException("Товар не знайдено");

        if (product.SellerId != sellerUserId)
            throw new InvalidOperationException("Це не ваш товар");
        _productRepository.Delete(product);
    }

    public ProductResponse UpdateAsAdmin(Guid productId, CreateProductRequest request)
    {
        var product = _productRepository.GetById(productId)
            ?? throw new InvalidOperationException("Товар не знайдено");

        product.ChangeTitle(request.Title);
        product.ChangeDescription(request.Description);
        product.SetPrice(request.Price);
        product.SetStockQuantity(request.StockQuantity);
        product.ChangeCategory(request.CategoryId);
        product.SetImageUrl(request.ImageUrl);

        _productRepository.Update(product);
        return ToResponse(product);
    }

    public void DeleteAsAdmin(Guid productId)
    {
        var product = _productRepository.GetById(productId)
            ?? throw new InvalidOperationException("Товар не знайдено");

        _productRepository.Delete(product);
    }

    private ProductResponse ToResponse(Product p)
    {
        var reviews = _reviewRepository.GetByProduct(p.Id).ToList();
        var reviewsCount = reviews.Count;
        var averageRating = reviewsCount == 0 ? 0 : Math.Round(reviews.Average(r => r.Rating), 2);

        return new ProductResponse
        {
            Id = p.Id,
            SellerId = p.SellerId,
            CategoryId = p.CategoryId,
            Title = p.Title,
            Description = p.Description,
            ImageUrl = p.ImageUrl,
            Price = p.Price,
            StockQuantity = p.StockQuantity,
            IsActive = p.IsActive,
            AverageRating = averageRating,
            ReviewsCount = reviewsCount
        };
    }
}
