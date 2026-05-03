using Marketplace.API.DTOs;

namespace Marketplace.API.Interfaces;

public interface IProductService
{
    IEnumerable<ProductResponse> GetAll();
    IEnumerable<ProductResponse> GetAllForAdmin();
    ProductResponse? GetById(Guid id);
    ProductResponse Create(CreateProductRequest request);

    ProductResponse CreateAsSeller(Guid sellerUserId, CreateProductRequest request);

    ProductResponse UpdateAsSeller(Guid sellerUserId, Guid productId, CreateProductRequest request);
    ProductResponse UpdateAsAdmin(Guid productId, CreateProductRequest request);

    void DeleteAsSeller(Guid sellerUserId, Guid productId);
    void DeleteAsAdmin(Guid productId);
}
