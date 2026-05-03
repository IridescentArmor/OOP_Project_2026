using System.ComponentModel.DataAnnotations;

namespace Marketplace.API.DTOs;

public class CreateReviewRequest
{
    [Range(1, 5, ErrorMessage = "Оцінка має бути від 1 до 5")]
    public int Rating { get; set; }

    [Required]
    [MinLength(3)]
    [MaxLength(2000)]
    public string Text { get; set; } = string.Empty;
}

