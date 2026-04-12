using Marketplace.API.Models;

namespace Marketplace.API.Interfaces;

public interface ICategoryRepository
{
    IEnumerable<Category> GetAll();
    Category? GetById(Guid id);
    void Add(Category category);
}
