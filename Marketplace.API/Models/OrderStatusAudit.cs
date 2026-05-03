namespace Marketplace.API.Models;

public static class OrderStatusAudit
{
    public static void RecordTransition(object? sender, OrderStatusChangedEventArgs e)
    {
    }
}
