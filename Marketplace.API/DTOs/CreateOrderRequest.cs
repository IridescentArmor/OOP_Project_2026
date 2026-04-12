using System.ComponentModel.DataAnnotations;

namespace Marketplace.API.DTOs;

public class CreateOrderRequest : IValidatableObject
{
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "Кошик обов'язковий")]
    public Dictionary<Guid, int>? Items { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var items = Items;
        if (items == null || items.Count == 0)
        {
            yield return new ValidationResult("Кошик не може бути порожнім", new[] { nameof(Items) });
            yield break;
        }

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
