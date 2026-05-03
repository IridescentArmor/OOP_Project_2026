using Marketplace.API.DTOs;
using Marketplace.API.Interfaces;
using Marketplace.API.Models;

namespace Marketplace.API.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;

    public CategoryService(ICategoryRepository categoryRepository, IProductRepository productRepository)
    {
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
    }

    public IReadOnlyList<CategoryResponse> GetAll()
    {
        var categories = _categoryRepository.GetAll().ToList();
        return BuildTree(categories, null);
    }

    public CategoryResponse? GetById(Guid id)
    {
        var category = _categoryRepository.GetById(id);
        return category == null ? null : ToResponse(category);
    }

    public CategoryResponse Create(CreateCategoryRequest request)
    {
        var name = request.Name.Trim();
        var categories = _categoryRepository.GetAll().ToList();
        var exists = categories.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (exists)
            throw new InvalidOperationException("Категорія з такою назвою вже існує");

        ValidateParent(request.ParentCategoryId, categories, null);

        var category = new Category(name, request.ParentCategoryId);
        _categoryRepository.Add(category);
        return ToResponse(category);
    }

    public CategoryResponse Update(Guid id, CreateCategoryRequest request)
    {
        var category = _categoryRepository.GetById(id)
            ?? throw new InvalidOperationException("Категорію не знайдено");

        var name = request.Name.Trim();
        var categories = _categoryRepository.GetAll().ToList();
        var exists = categories.Any(c => c.Id != id && c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (exists)
            throw new InvalidOperationException("Категорія з такою назвою вже існує");

        ValidateParent(request.ParentCategoryId, categories, id);

        category.ChangeName(name);
        category.ChangeParent(request.ParentCategoryId);
        _categoryRepository.Update(category);
        return ToResponse(category);
    }

    public void Delete(Guid id)
    {
        var category = _categoryRepository.GetById(id)
            ?? throw new InvalidOperationException("Категорію не знайдено");

        var hasActiveProducts = _productRepository.GetAll()
            .Any(p => p.CategoryId == id && p.IsActive);
        if (hasActiveProducts)
            throw new InvalidOperationException("Не можна видалити категорію, якщо до неї прив'язані активні товари");

        var hasChildren = _categoryRepository.GetAll().Any(c => c.ParentCategoryId == id);
        if (hasChildren)
            throw new InvalidOperationException("Не можна видалити категорію, поки в ній є підкатегорії");

        _categoryRepository.Delete(category);
    }

    private static void ValidateParent(Guid? parentCategoryId, IReadOnlyCollection<Category> categories, Guid? currentCategoryId)
    {
        if (parentCategoryId == null)
            return;

        if (parentCategoryId == currentCategoryId)
            throw new InvalidOperationException("Категорія не може бути батьківською сама для себе");

        var parent = categories.FirstOrDefault(c => c.Id == parentCategoryId)
            ?? throw new InvalidOperationException("Батьківську категорію не знайдено");

        if (currentCategoryId == null)
            return;

        var parentId = parent.ParentCategoryId;
        while (parentId != null)
        {
            if (parentId == currentCategoryId)
                throw new InvalidOperationException("Не можна створити циклічну структуру підкатегорій");

            parentId = categories.First(c => c.Id == parentId.Value).ParentCategoryId;
        }
    }

    private static IReadOnlyList<CategoryResponse> BuildTree(IReadOnlyList<Category> categories, Guid? parentCategoryId)
        => categories
            .Where(c => c.ParentCategoryId == parentCategoryId)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                ParentCategoryId = c.ParentCategoryId,
                Children = BuildTree(categories, c.Id)
            })
            .ToList();

    private static CategoryResponse ToResponse(Category c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        ParentCategoryId = c.ParentCategoryId
    };
}
