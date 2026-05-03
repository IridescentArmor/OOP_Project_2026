namespace Marketplace.API.DTOs;

public class CategoryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    public IReadOnlyList<CategoryResponse> Children { get; set; } = Array.Empty<CategoryResponse>();
}
