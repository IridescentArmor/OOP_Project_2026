using Marketplace.API.Data;
using Marketplace.API.Interfaces;
using Marketplace.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.API.Repositories;

public class EfUserRepository : IUserRepository
{
    private readonly MarketplaceDbContext _context;

    public EfUserRepository(MarketplaceDbContext context)
    {
        _context = context;
    }

    public IEnumerable<User> GetAll()
    {
        return _context.Users
            .AsNoTracking()
            .Select(u => u.Id)
            .ToList()
            .Select(id => GetById(id))
            .Where(u => u != null)
            .Cast<User>()
            .ToList();
    }

    public User? GetById(Guid id)
    {
        var user = _context.Users.AsNoTracking().FirstOrDefault(u => u.Id == id);
        if (user == null)
            return null;

        HydrateRoles(user);
        return user;
    }

    public User? GetByEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var key = email.Trim().ToLowerInvariant();
        var user = _context.Users.AsNoTracking().FirstOrDefault(u => u.Email == key);
        if (user == null)
            return null;

        HydrateRoles(user);
        return user;
    }

    public void Add(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user), "Користувач не може бути порожнім");

        _context.Users.Add(user);
        PersistRoleRows(user);
        _context.SaveChanges();
    }

    public void UpdateStoredRoles(User user)
    {
        var existing = _context.UserRoleRows.Where(r => r.UserId == user.Id).ToList();
        _context.UserRoleRows.RemoveRange(existing);
        PersistRoleRows(user);
        _context.SaveChanges();
    }

    public void Update(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user), "Користувач не може бути порожнім");

        var tracked = _context.Users.Local.FirstOrDefault(u => u.Id == user.Id);
        if (tracked != null)
        {
            _context.Entry(tracked).CurrentValues.SetValues(user);
        }
        else
        {
            _context.Users.Attach(user);
            _context.Entry(user).State = EntityState.Modified;
        }
        _context.SaveChanges();
    }

    public void Delete(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user), "Користувач не може бути порожнім");

        var roles = _context.UserRoleRows.Where(r => r.UserId == user.Id).ToList();
        if (roles.Count > 0)
            _context.UserRoleRows.RemoveRange(roles);

        _context.Users.Remove(user);
        _context.SaveChanges();
    }

    private void HydrateRoles(User user)
    {
        if (user.Roles.Count > 0)
            return;

        var rows = _context.UserRoleRows.AsNoTracking().Where(r => r.UserId == user.Id).ToList();
        foreach (var row in rows)
            user.AddRole(MapRowToRole(row));

        var customer = user.Roles.OfType<CustomerRole>().FirstOrDefault();
        if (customer != null)
        {
            var orders = _context.Orders
                .AsNoTracking()
                .Where(o => o.UserId == user.Id)
                .Include(o => o.Items)
                .ToList();
            customer.AttachLoadedOrders(orders);
        }

        var seller = user.Roles.OfType<SellerRole>().FirstOrDefault();
        if (seller != null)
        {
            var products = _context.Products.Where(p => p.SellerId == user.Id).ToList();
            foreach (var p in products)
                seller.AddProduct(p);
        }
    }

    private static IRole MapRowToRole(UserRoleRow row)
    {
        return row.RoleType switch
        {
            "Customer" => CreateCustomer(row),
            "Seller" => CreateSeller(row),
            "Admin" => new AdminRole(row.AdminAccessLevel ?? 0),
            _ => throw new InvalidOperationException($"Невідомий тип ролі: {row.RoleType}")
        };
    }

    private static CustomerRole CreateCustomer(UserRoleRow row)
    {
        var c = new CustomerRole();
        c.HydrateFromDatabase(row.CustomerDefaultAddress, row.CustomerLoyaltyPoints);
        return c;
    }

    private static SellerRole CreateSeller(UserRoleRow row)
    {
        var name = row.SellerCompanyName ?? throw new InvalidOperationException("У ролі продавця відсутня назва компанії в БД");
        var s = new SellerRole(name);
        s.SetRatingForPersistence(row.SellerRating);
        s.SetApprovalForPersistence(row.SellerIsApproved);
        return s;
    }

    private void PersistRoleRows(User user)
    {
        foreach (var role in user.Roles)
        {
            var row = new UserRoleRow { Id = Guid.NewGuid(), UserId = user.Id };
            switch (role)
            {
                case CustomerRole cr:
                    row.RoleType = "Customer";
                    row.CustomerDefaultAddress = cr.DefaultAddress;
                    row.CustomerLoyaltyPoints = cr.LoyaltyPoints;
                    break;
                case SellerRole sr:
                    row.RoleType = "Seller";
                    row.SellerCompanyName = sr.CompanyName;
                    row.SellerRating = sr.Rating;
                    row.SellerIsApproved = sr.IsApproved;
                    break;
                case AdminRole ar:
                    row.RoleType = "Admin";
                    row.AdminAccessLevel = ar.AccessLevel;
                    break;
                default:
                    throw new InvalidOperationException($"Непідтримувана роль для збереження: {role.GetType().Name}");
            }

            _context.UserRoleRows.Add(row);
        }
    }
}
