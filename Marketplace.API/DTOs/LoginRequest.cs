using System.ComponentModel.DataAnnotations;

namespace Marketplace.API.DTOs;

public class LoginRequest
{
    [Required(ErrorMessage = "Email обов'язковий")]
    [EmailAddress(ErrorMessage = "Некоректний формат email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Пароль обов'язковий")]
    [MinLength(1, ErrorMessage = "Пароль не може бути порожнім")]
    public string Password { get; set; } = string.Empty;
}
