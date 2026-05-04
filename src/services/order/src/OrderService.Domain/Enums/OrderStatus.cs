namespace OrderService.Domain.Enums;

public enum OrderStatus { Pending = 0, Confirmed = 1, Processing = 2, Shipped = 3, Delivered = 4, Cancelled = 5 }
public enum PaymentStatus { Pending = 0, Paid = 1, Failed = 2, Refunded = 3 }