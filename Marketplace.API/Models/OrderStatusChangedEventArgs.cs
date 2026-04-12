using System;

namespace Marketplace.API.Models
{
    public class OrderStatusChangedEventArgs : EventArgs
    {
        public Guid OrderId { get; }
        public OrderStatus OldStatus { get; }
        public OrderStatus NewStatus { get; }

        public OrderStatusChangedEventArgs(Guid orderId, OrderStatus oldStatus, OrderStatus newStatus)
        {
            OrderId = orderId;
            OldStatus = oldStatus;
            NewStatus = newStatus;
        }
    }
}