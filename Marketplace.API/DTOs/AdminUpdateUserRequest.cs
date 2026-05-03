using System.ComponentModel.DataAnnotations;
using Marketplace.API.Validation;

namespace Marketplace.API.DTOs;

public class AdminUpdateUserRequest : IValidatableObject
{
    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(10)]
    [MaxLength(32)]
    public string PhoneNumber { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!ValidationRules.IsValidPersonName(Name))
            yield return new ValidationResult("Ім'я може містити лише літери, пробіли, апостроф і дефіс", new[] { nameof(Name) });

        if (!ValidationRules.IsValidPhoneNumber(PhoneNumber))
            yield return new ValidationResult("Вкажіть коректний номер телефону без літер", new[] { nameof(PhoneNumber) });
    }
}
