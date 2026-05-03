using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Marketplace.API.DTOs;

public class CreateProductRequest : IValidatableObject
{
    [Required(ErrorMessage = "Назва обов'язкова")]
    [MinLength(1, ErrorMessage = "Назва обов'язкова")]
    [MaxLength(200, ErrorMessage = "Назва занадто довга")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "Опис занадто довгий")]
    public string Description { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Кількість на складі не може бути від'ємною")]
    public int StockQuantity { get; set; }

    [Required(ErrorMessage = "Ідентифікатор категорії обов'язковий")]
    public Guid CategoryId { get; set; }

    [Required(ErrorMessage = "Ідентифікатор продавця обов'язковий")]
    public Guid SellerId { get; set; }

    public decimal Price { get; set; }

    [MaxLength(500, ErrorMessage = "Посилання на зображення занадто довге")]
    public string? ImageUrl { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Price <= 0)
            yield return new ValidationResult("Ціна має бути більшою за нуль", new[] { nameof(Price) });
    }
}
