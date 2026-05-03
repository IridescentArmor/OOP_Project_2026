using System.ComponentModel.DataAnnotations;
using Marketplace.API.Validation;

namespace Marketplace.API.DTOs;

public class CreateOrderRequest : IValidatableObject
{
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "Кошик обов'язковий")]
    public Dictionary<Guid, int>? Items { get; set; }

    [Required(ErrorMessage = "Ім'я отримувача обов'язкове")]
    [MinLength(2, ErrorMessage = "Ім'я отримувача занадто коротке")]
    [MaxLength(100, ErrorMessage = "Ім'я отримувача занадто довге")]
    public string RecipientName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Телефон отримувача обов'язковий")]
    [MinLength(10, ErrorMessage = "Телефон отримувача некоректний")]
    [MaxLength(32, ErrorMessage = "Телефон отримувача занадто довгий")]
    public string RecipientPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Адреса доставки обов'язкова")]
    [MinLength(5, ErrorMessage = "Адреса доставки занадто коротка")]
    [MaxLength(300, ErrorMessage = "Адреса доставки занадто довга")]
    public string ShippingAddress { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var items = Items;
        if (items == null || items.Count == 0)
        {
            yield return new ValidationResult("Кошик не може бути порожнім", new[] { nameof(Items) });
            yield break;
        }

        if (string.IsNullOrWhiteSpace(RecipientName))
            yield return new ValidationResult("Ім'я отримувача обов'язкове", new[] { nameof(RecipientName) });

        if (string.IsNullOrWhiteSpace(RecipientPhone))
            yield return new ValidationResult("Телефон отримувача обов'язковий", new[] { nameof(RecipientPhone) });
        else if (!ValidationRules.IsValidPhoneNumber(RecipientPhone))
            yield return new ValidationResult("Вкажіть коректний телефон отримувача без літер", new[] { nameof(RecipientPhone) });

        if (!string.IsNullOrWhiteSpace(RecipientName) && !ValidationRules.IsValidPersonName(RecipientName))
            yield return new ValidationResult("Ім'я отримувача містить недопустимі символи", new[] { nameof(RecipientName) });

        if (string.IsNullOrWhiteSpace(ShippingAddress))
            yield return new ValidationResult("Адреса доставки обов'язкова", new[] { nameof(ShippingAddress) });

        foreach (var kv in items)
        {
            if (kv.Value <= 0)
            {
                yield return new ValidationResult("Кількість кожного товару має бути більше 0", new[] { nameof(Items) });
                yield break;
            }
        }
    }
}
