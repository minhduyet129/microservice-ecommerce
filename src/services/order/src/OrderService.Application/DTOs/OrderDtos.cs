namespace OrderService.Application.DTOs;

public record OrderItemDto(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);
public record CreateOrderRequest(string UserId, string UserEmail, string ShippingAddress, string ShippingPhone, List<OrderItemDto> Items);
public record OrderDto(Guid Id, string UserId, string UserEmail, string Status, string PaymentStatus, decimal TotalAmount, string? ShippingAddress, List<OrderItemDto> Items, DateTime CreatedAt);