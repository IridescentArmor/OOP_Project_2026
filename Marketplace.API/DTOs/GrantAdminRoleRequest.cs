using System.ComponentModel.DataAnnotations;

namespace Marketplace.API.DTOs;

public class GrantAdminRoleRequest
{
    [Range(1, 100, ErrorMessage = "Рівень доступу має бути в межах 1..100")]
    public int AccessLevel { get; set; } = 10;
}
