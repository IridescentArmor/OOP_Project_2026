using System;

namespace Marketplace.API.Models
{
    public class OrderItem : IEntity
    {
        private OrderItem()
        {
        }

        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid OrderId { get; private set; }
        public Guid ProductId { get; private set; }
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }

        public OrderItem(Guid orderId, Guid productId, int quantity, decimal unitPrice)
        {
            if (quantity <= 0)
             throw new ArgumentException("Кількість має бути більше 0");

            if (unitPrice <= 0)
              throw new ArgumentException("Ціна має бути більше 0");

            OrderId = orderId;
            ProductId = productId;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }

        public decimal CalculateSubtotal()
        {
            return Quantity * UnitPrice;
        }
    }
}