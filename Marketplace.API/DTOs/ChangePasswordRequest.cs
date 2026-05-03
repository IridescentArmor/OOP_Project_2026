using System.ComponentModel.DataAnnotations;

namespace Marketplace.API.DTOs;

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(8, ErrorMessage = "Новий пароль має містити щонайменше 8 символів")]
    [MaxLength(100)]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "Новий пароль має містити літери та цифри")]
    public string NewPassword { get; set; } = string.Empty;
}
