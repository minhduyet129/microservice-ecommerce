using BuildingBlocks.Core.Abstractions;

namespace OrderService.Domain.Entities;

public class OrderItem : Entity<Guid>
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;

    public OrderItem()
    {
        Id = Guid.NewGuid();
    }

    public static OrderItem Create(Guid orderId, Guid productId, string productName, decimal unitPrice, int quantity)
    {
        return new OrderItem { OrderId = orderId, ProductId = productId, ProductName = productName, UnitPrice = unitPrice, Quantity = quantity, CreatedAt = DateTime.UtcNow };
    }
}