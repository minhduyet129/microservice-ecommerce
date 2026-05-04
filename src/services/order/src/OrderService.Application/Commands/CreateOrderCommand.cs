using FluentValidation;
using MediatR;
using OrderService.Application.DTOs;
using OrderService.Application.Services;
using OrderService.Domain.Entities;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Commands;

public record CreateOrderCommand(CreateOrderRequest Request) : IRequest<OrderDto>;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Request.UserId).NotEmpty();
        RuleFor(x => x.Request.UserEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.Request.ShippingAddress).NotEmpty();
        RuleFor(x => x.Request.ShippingPhone).NotEmpty();
        RuleFor(x => x.Request.Items).NotEmpty();
    }
}

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductGrpcClient _productClient;

    public CreateOrderCommandHandler(IOrderRepository orderRepository, IProductGrpcClient productClient)
    {
        _orderRepository = orderRepository;
        _productClient = productClient;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var order = Order.Create(request.Request.UserId, request.Request.UserEmail, request.Request.ShippingAddress, request.Request.ShippingPhone);

        foreach (var item in request.Request.Items)
        {
            var product = await _productClient.GetProductAsync(item.ProductId);
            if (product == null) throw new Exception($"Product {item.ProductId} not found");
            order.AddItem(item.ProductId, product.Name, (decimal)product.Price, item.Quantity);
        }

        var createdOrder = await _orderRepository.AddAsync(order, ct);

        foreach (var item in request.Request.Items)
        {
            await _productClient.ReduceStockAsync(item.ProductId, item.Quantity);
        }

        return new OrderDto(createdOrder.Id, createdOrder.UserId, createdOrder.UserEmail, createdOrder.Status.ToString(), createdOrder.PaymentStatus.ToString(), createdOrder.TotalAmount, createdOrder.ShippingAddress, createdOrder.Items.Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity)).ToList(), createdOrder.CreatedAt);
    }
}