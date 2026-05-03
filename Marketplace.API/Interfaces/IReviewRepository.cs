using Marketplace.API.Models;

namespace Marketplace.API.Interfaces;

public interface IReviewRepository
{
    void Add(Review review);
    void Update(Review review);
    void Delete(Review review);
    Review? GetById(Guid id);
    IEnumerable<Review> GetAll();
    IEnumerable<Review> GetByProduct(Guid productId, bool includeHidden = false);
}

