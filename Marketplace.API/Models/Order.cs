using System;
using System.Collections.Generic;
using System.Linq;
using Marketplace.API.Validation;

namespace Marketplace.API.Models
{
    public class Order : IEntity
    {
        private Order()
        {
        }

        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid UserId { get; private set; }
        public Guid SellerId { get; private set; }
        public OrderStatus Status { get; private set; } = OrderStatus.Created;
        public string? TrackingNumber { get; private set; }
        public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
        public string RecipientName { get; private set; } = string.Empty;
        public string RecipientPhone { get; private set; } = string.Empty;
        public string ShippingAddress { get; private set; } = string.Empty;
        public List<OrderItem> Items { get; private set; } = new List<OrderItem>();

        public event OrderStatusChangedHandler? StatusChanged;

        public Order(Guid userId, Guid sellerId)
        {
            UserId = userId;
            SellerId = sellerId;
        }

        public void SetShippingInfo(string recipientName, string recipientPhone, string shippingAddress)
        {
            if (!ValidationRules.IsValidPersonName(recipientName))
                throw new ArgumentException("Некоректне ім'я отримувача");
            if (!ValidationRules.IsValidPhoneNumber(recipientPhone))
                throw new ArgumentException("Некоректний телефон отримувача");
            if (string.IsNullOrWhiteSpace(shippingAddress) || shippingAddress.Trim().Length < 5)
                throw new ArgumentException("Некоректна адреса доставки");

            RecipientName = recipientName.Trim();
            RecipientPhone = ValidationRules.NormalizePhoneNumber(recipientPhone);
            ShippingAddress = shippingAddress.Trim();
        }
        private bool IsValidTransition(OrderStatus current, OrderStatus next)
        {
            return current switch
            {
                OrderStatus.Created => next == OrderStatus.Processing || next == OrderStatus.Canceled,
                OrderStatus.Processing => next == OrderStatus.Shipped || next == OrderStatus.Canceled,
                OrderStatus.Shipped => next == OrderStatus.Completed,
                _ => false
            };
        }
        public void AddItem(Product product, int quantity)
        {
            if (product == null || quantity <= 0) 
                throw new ArgumentException("Некоректні дані товару");
            
            var orderItem = new OrderItem(this.Id, product.Id, quantity, product.GetPrice());
            Items.Add(orderItem);
        }

        public decimal CalculateTotal()
        {
            return Items.Sum(item => item.CalculateSubtotal());
        }

        public void ChangeStatus(OrderStatus newStatus, string? trackingNumber = null)
        {
            if (!IsValidTransition(Status, newStatus))
                throw new InvalidOperationException("Невірний перехід статусу");

            if (newStatus == OrderStatus.Shipped && string.IsNullOrWhiteSpace(trackingNumber))
                throw new InvalidOperationException("Для статусу 'Відправлено' необхідно вказати ТТН");

            var oldStatus = Status;
            Status = newStatus;

            if (newStatus == OrderStatus.Shipped)
                TrackingNumber = trackingNumber!.Trim();

            StatusChanged?.Invoke(this,
                new OrderStatusChangedEventArgs(this.Id, oldStatus, newStatus));
        }
    }
}