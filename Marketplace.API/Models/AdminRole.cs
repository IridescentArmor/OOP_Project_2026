using System;
using System.Linq;

namespace Marketplace.API.Models
{
    public class AdminRole : IRole
    {
        public UserRoleKind Kind => UserRoleKind.Admin;
        public string RoleName => "Admin";

        public int AccessLevel { get; private set; }

        public AdminRole(int accessLevel)
        {
            AccessLevel = accessLevel;
        }

        public void BlockUser(User user)
        {
            if (AccessLevel < 10)
                throw new UnauthorizedAccessException("Недостатній рівень доступу");

            if (user == null)
                throw new ArgumentNullException(nameof(user), "Користувач не може бути порожнім");

            if (user.HasRole<AdminRole>())
                throw new InvalidOperationException("Не можна заблокувати адміністратора");

        }

        public void UnblockUser(User user)
        {
            if (AccessLevel < 10)
                throw new UnauthorizedAccessException("Недостатній рівень доступу");

            if (user == null)
                throw new ArgumentNullException(nameof(user), "Користувач не може бути порожнім");

            if (user.HasRole<AdminRole>())
                throw new InvalidOperationException("Не можна змінювати статус адміністратора");

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
    }
}
