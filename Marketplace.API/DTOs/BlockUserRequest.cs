using System.ComponentModel.DataAnnotations;

namespace Marketplace.API.DTOs;

public class BlockUserRequest
{
    [Required]
    [MinLength(3)]
    [MaxLength(300)]
    public string Reason { get; set; } = string.Empty;
}

