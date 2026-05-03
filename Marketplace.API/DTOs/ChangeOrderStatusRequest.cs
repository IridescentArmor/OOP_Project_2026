using System.ComponentModel.DataAnnotations;

namespace Marketplace.API.DTOs;

public class ChangeOrderStatusRequest
{
    [Required(ErrorMessage = "Статус обов'язковий")]
    public string Status { get; set; } = string.Empty;

    public string? TrackingNumber { get; set; }
}

