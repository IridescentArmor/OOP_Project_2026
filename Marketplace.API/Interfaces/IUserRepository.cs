using Marketplace.API.Models;

namespace Marketplace.API.Interfaces;

public interface IUserRepository
{
    IEnumerable<User> GetAll();

    User? GetById(Guid id);

    User? GetByEmail(string email);

    void Add(User user);

    void UpdateStoredRoles(User user);

    void Update(User user);

    void Delete(User user);
}
