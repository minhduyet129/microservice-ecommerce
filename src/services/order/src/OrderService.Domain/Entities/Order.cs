using BuildingBlocks.Core.Abstractions;
using BuildingBlocks.Core.Events;
using OrderService.Domain.Enums;

namespace OrderService.Domain.Entities;

public class Order : IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public IReadOnlyList<IDomainEvent> GetDomainEvents() => _domainEvents.AsReadOnly();
    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public decimal TotalAmount { get; set; }
    public string? ShippingAddress { get; set; }
    public string? ShippingPhone { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<OrderItem> Items { get; set; } = new();

    public Order() { Id = Guid.NewGuid(); }

    public static Order Create(string userId, string userEmail, string shippingAddress, string shippingPhone)
    {
        var order = new Order { Id = Guid.NewGuid(), UserId = userId, UserEmail = userEmail, ShippingAddress = shippingAddress, ShippingPhone = shippingPhone, Status = OrderStatus.Pending, PaymentStatus = PaymentStatus.Pending, CreatedAt = DateTime.UtcNow };
        order.AddDomainEvent(new OrderCreatedEvent(order.Id, order.UserId, order.TotalAmount));
        return order;
    }

    public void AddItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        var orderItem = OrderItem.Create(Id, productId, productName, unitPrice, quantity);
        Items.Add(orderItem);
        RecalculateTotal();
    }

    public void RecalculateTotal() => TotalAmount = Items.Sum(i => i.TotalPrice);

    public void Confirm() { Status = OrderStatus.Confirmed; UpdatedAt = DateTime.UtcNow; AddDomainEvent(new OrderConfirmedEvent(Id)); }

    public void Cancel(string reason) { Status = OrderStatus.Cancelled; UpdatedAt = DateTime.UtcNow; AddDomainEvent(new OrderCancelledEvent(Id, reason)); }

    public void MarkAsPaid() { PaymentStatus = PaymentStatus.Paid; Status = OrderStatus.Processing; UpdatedAt = DateTime.UtcNow; }

    public void ClearDomainEvents() => _domainEvents.Clear();
}

public record OrderCreatedEvent(Guid OrderId, string UserId, decimal TotalAmount) : IDomainEvent
{
    public DateTime OccurredOn => DateTime.UtcNow;
}

public record OrderConfirmedEvent(Guid OrderId) : IDomainEvent
{
    public DateTime OccurredOn => DateTime.UtcNow;
}

public record OrderCancelledEvent(Guid OrderId, string Reason) : IDomainEvent
{
    public DateTime OccurredOn => DateTime.UtcNow;
}