using Marketplace.API.Models;

namespace Marketplace.Tests.Domain;

public class OrderStatusEventTests
{
    [Fact]
    public void ChangeStatus_InvokesStatusChanged_WithDelegatePayload()
    {
        var order = new Order(Guid.NewGuid(), Guid.NewGuid());
        OrderStatusChangedEventArgs? captured = null;
        OrderStatusChangedHandler handler = (_, e) => captured = e;
        order.StatusChanged += handler;

        order.ChangeStatus(OrderStatus.Processing);

        order.StatusChanged -= handler;
        Assert.NotNull(captured);
        Assert.Equal(order.Id, captured!.OrderId);
        Assert.Equal(OrderStatus.Created, captured.OldStatus);
        Assert.Equal(OrderStatus.Processing, captured.NewStatus);
    }
}
