using System.ComponentModel.DataAnnotations;

namespace Marketplace.API.DTOs;

public class CreateCategoryRequest
{
    [Required(ErrorMessage = "Назва категорії обов'язкова")]
    [MinLength(1, ErrorMessage = "Назва не може бути порожньою")]
    [MaxLength(100, ErrorMessage = "Назва занадто довга")]
    public string Name { get; set; } = string.Empty;

    public Guid? ParentCategoryId { get; set; }
}
