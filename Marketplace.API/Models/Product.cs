using System;

namespace Marketplace.API.Models
{
    public class Product : IEntity, IOrderable
    {
        private Product()
        {
            Title = string.Empty;
            Description = string.Empty;
        }

        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Title { get; private set; }
        public string Description { get; private set; }
        public decimal Price { get; private set; }
        public int StockQuantity { get; private set; }
        public Guid SellerId { get; private set; }
        public Guid CategoryId { get; private set; }

        public Product(string title, string description, decimal price, int stockQuantity, Guid sellerId, Guid categoryId)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Назва товару обов'язкова");
            Title = title;
            Description = description;
            SetPrice(price);
            if (stockQuantity < 0)
            throw new ArgumentException("Кількість не може бути від'ємною");

            StockQuantity = stockQuantity;
            SellerId = sellerId;
            CategoryId = categoryId;
        }

        public decimal GetPrice() => Price;

        public void SetPrice(decimal newPrice)
        {
            if (newPrice <= 0) throw new ArgumentException("Ціна має бути більшою за нуль");
            Price = newPrice;
        }

        public void UpdateStock(int amount)
        {
            if (StockQuantity + amount < 0)
                throw new InvalidOperationException("Недостатньо товару на складі");
            
            StockQuantity += amount;
        }
    }
}