using System.ComponentModel.DataAnnotations;

namespace Marketplace.API.DTOs;

public class CreateUserRequest : IValidatableObject
{
    [Required(ErrorMessage = "Ім'я обов'язкове")]
    [MinLength(2, ErrorMessage = "Ім'я має містити щонайменше 2 символи")]
    [MaxLength(100, ErrorMessage = "Ім'я занадто довге")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email обов'язковий")]
    [EmailAddress(ErrorMessage = "Некоректний формат email")]
    [MaxLength(256, ErrorMessage = "Email занадто довгий")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Номер телефону обов'язковий")]
    [MinLength(5, ErrorMessage = "Некоректний номер телефону")]
    [MaxLength(32, ErrorMessage = "Номер телефону занадто довгий")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Хеш пароля обов'язковий")]
    [MinLength(1, ErrorMessage = "Хеш пароля не може бути порожнім")]
    [MaxLength(500, ErrorMessage = "Хеш пароля занадто довгий")]
    public string PasswordHash { get; set; } = string.Empty;

    public bool IncludeCustomerRole { get; set; }

    public bool IncludeSellerRole { get; set; }

    public bool IncludeAdminRole { get; set; }

    [MaxLength(200, ErrorMessage = "Назва компанії занадто довга")]
    public string? SellerCompanyName { get; set; }

    [Range(0, 100, ErrorMessage = "Рівень доступу адміна має бути від 0 до 100")]
    public int AdminAccessLevel { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!IncludeCustomerRole && !IncludeSellerRole && !IncludeAdminRole)
            yield return new ValidationResult("Оберіть хоча б одну роль", new[] { nameof(IncludeCustomerRole) });

        if (IncludeSellerRole && string.IsNullOrWhiteSpace(SellerCompanyName))
            yield return new ValidationResult("Для ролі продавця вкажіть назву компанії", new[] { nameof(SellerCompanyName) });
    }
}
