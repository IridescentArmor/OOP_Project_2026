using Marketplace.API.DTOs;
using Marketplace.API.Interfaces;
using Marketplace.API.Models;

namespace Marketplace.API.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public IReadOnlyList<CategoryResponse> GetAll()
        => _categoryRepository.GetAll().Select(ToResponse).ToList();

    public CategoryResponse? GetById(Guid id)
    {
        var category = _categoryRepository.GetById(id);
        return category == null ? null : ToResponse(category);
    }

    public CategoryResponse Create(CreateCategoryRequest request)
    {
        var name = request.Name.Trim();
        var exists = _categoryRepository.GetAll()
            .Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (exists)
            throw new InvalidOperationException("Категорія з такою назвою вже існує");

        var category = new Category(name);
        _categoryRepository.Add(category);
        return ToResponse(category);
    }

    private static CategoryResponse ToResponse(Category c) => new()
    {
        Id = c.Id,
        Name = c.Name
    };
}
