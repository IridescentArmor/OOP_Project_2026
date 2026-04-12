namespace Marketplace.API.Models;

public interface IRole
{
    UserRoleKind Kind { get; }
    string RoleName { get; }
}
