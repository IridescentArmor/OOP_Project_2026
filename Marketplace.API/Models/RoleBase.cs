using System.Collections.Generic;
using System.Linq;

namespace Marketplace.API.Models;

public abstract class RoleBase : IRole
{
    public abstract UserRoleKind Kind { get; }

    public abstract string RoleName { get; }

    public static IReadOnlyDictionary<UserRoleKind, string> UkrainianLabels { get; } =
        new Dictionary<UserRoleKind, string>
        {
            [UserRoleKind.Admin] = "Адміністратор",
            [UserRoleKind.Seller] = "Продавець",
            [UserRoleKind.Customer] = "Покупець",
        };

    protected RoleBase()
    {
    }

    protected static void EnsureUserNotNull(User? user, string parameterName)
        => ArgumentNullException.ThrowIfNull(user, parameterName);

    public static HashSet<UserRoleKind> DistinctKinds(IEnumerable<IRole> roles)
        => roles.Select(r => r.Kind).ToHashSet();
}
