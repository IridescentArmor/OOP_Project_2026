using Marketplace.API.DTOs;

namespace Marketplace.API.Interfaces;

public interface ICategoryService
{
    IReadOnlyList<CategoryResponse> GetAll();
    CategoryResponse? GetById(Guid id);
    CategoryResponse Create(CreateCategoryRequest request);

    CategoryResponse Update(Guid id, CreateCategoryRequest request);

    void Delete(Guid id);
}
