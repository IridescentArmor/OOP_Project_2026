using System;
using System.Collections.Generic;
using System.Linq;

namespace Marketplace.API.Models
{
    public class Order : IEntity
    {
        private Order()
        {
        }

        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid UserId { get; private set; }
        public OrderStatus Status { get; private set; } = OrderStatus.Created;
        public List<OrderItem> Items { get; private set; } = new List<OrderItem>();

        public event EventHandler<OrderStatusChangedEventArgs>? OnStatusChanged;

        public Order(Guid userId)
        {
            UserId = userId;
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

        public void ChangeStatus(OrderStatus newStatus)
        {
            if (!IsValidTransition(Status, newStatus))
                throw new InvalidOperationException("Невірний перехід статусу");

            var oldStatus = Status;
            Status = newStatus;

            OnStatusChanged?.Invoke(this,
                new OrderStatusChangedEventArgs(this.Id, oldStatus, newStatus));
        }
    }
}