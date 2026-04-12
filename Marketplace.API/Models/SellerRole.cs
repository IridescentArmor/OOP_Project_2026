using System;
using System.Collections.Generic;
using System.Linq;

namespace Marketplace.API.Models
{
    public class SellerRole : IRole
    {
        public UserRoleKind Kind => UserRoleKind.Seller;
        public string RoleName => "Seller";

        public string CompanyName { get; private set; }
        public double Rating { get; private set; } = 0.0;
        public List<Product> Products { get; private set; } = new();

        public SellerRole(string companyName)
        {
            if (string.IsNullOrWhiteSpace(companyName))
                throw new ArgumentException("Назва компанії не може бути порожньою");

            CompanyName = companyName;
        }

        public void SetRatingForPersistence(double rating)
        {
            Rating = rating;
        }

        public void AddProduct(Product product)
        {
            if (product != null && product.Price > 0 && product.StockQuantity >= 0)
                Products.Add(product);
        }

        public void ChangeProductPrice(Product product, Guid sellerUserId, decimal newPrice)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product), "Товар не може бути порожнім");

            if (product.SellerId != sellerUserId)
                throw new InvalidOperationException("Це не ваш товар");

            product.SetPrice(newPrice);
        }

        public bool RemoveProduct(Guid productId)
        {
            var product = Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
                return false;

            Products.Remove(product);
            return true;
        }

        public void RestockProduct(Guid productId, int amount)
        {
            var product = Products.FirstOrDefault(p => p.Id == productId);
            if (product != null)
                product.UpdateStock(amount);
        }
    }
}
