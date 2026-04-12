using Marketplace.API.DTOs;

namespace Marketplace.API.Interfaces;

public interface IProductService
{
    IEnumerable<ProductResponse> GetAll();
    ProductResponse? GetById(Guid id);
    ProductResponse Create(CreateProductRequest request);
}
