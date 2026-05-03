using System.Linq;

namespace Marketplace.API.Models;

public class AdminRole : RoleBase
{
    public override UserRoleKind Kind => UserRoleKind.Admin;

    public override string RoleName => "Admin";

    public int AccessLevel { get; private set; }

    public AdminRole(int accessLevel)
    {
        ValidateAccessLevel(accessLevel);
        AccessLevel = accessLevel;
    }

    public void UpdateAccessLevel(int accessLevel)
    {
        ValidateAccessLevel(accessLevel);
        AccessLevel = accessLevel;
    }

    public void BlockUser(User user, string reason)
    {
        if (AccessLevel < 10)
            throw new UnauthorizedAccessException("Недостатній рівень доступу");

        EnsureUserNotNull(user, nameof(user));

        if (user.HasRole<AdminRole>())
            throw new InvalidOperationException("Не можна заблокувати адміністратора");

        user.Block(reason);
    }

    public void UnblockUser(User user)
    {
        if (AccessLevel < 10)
            throw new UnauthorizedAccessException("Недостатній рівень доступу");

        EnsureUserNotNull(user, nameof(user));

        if (user.HasRole<AdminRole>())
            throw new InvalidOperationException("Не можна змінювати статус адміністратора");

        user.Unblock();
    }

    public Category AddCategory(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Назва не може бути порожньою");

        return new Category(name);
    }

    public void RemoveCategory(Category category)
    {
        if (category == null)
            throw new ArgumentNullException(nameof(category), "Категорія не може бути порожньою");

        if (category.Products.Any())
            throw new InvalidOperationException("Категорія містить товари");
    }

    private static void ValidateAccessLevel(int accessLevel)
    {
        if (accessLevel <= 0 || accessLevel > 100)
            throw new ArgumentOutOfRangeException(nameof(accessLevel),
                "Рівень доступу адміністратора має бути в межах 1..100");
    }
}
