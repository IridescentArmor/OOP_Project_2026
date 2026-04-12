using Marketplace.API.DTOs;
using Marketplace.API.Interfaces;
using Marketplace.API.Models;

namespace Marketplace.API.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public IEnumerable<ProductResponse> GetAll()
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

        _productRepository.Add(product);
        return ToResponse(product);
    }

    private static ProductResponse ToResponse(Product p) => new()
    {
        Id = p.Id,
        Title = p.Title,
        Description = p.Description,
        Price = p.Price,
        StockQuantity = p.StockQuantity
    };
}
