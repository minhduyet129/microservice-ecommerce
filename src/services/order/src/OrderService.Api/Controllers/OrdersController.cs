using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Commands;
using OrderService.Application.DTOs;
using OrderService.Domain.Repositories;

namespace OrderService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IOrderRepository _orderRepository;

    public OrdersController(IMediator mediator, IOrderRepository orderRepository) { _mediator = mediator; _orderRepository = orderRepository; }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderRequest request)
    {
        var userId = User.FindFirst("sub")?.Value ?? request.UserId;
        var userEmail = User.FindFirst("email")?.Value ?? request.UserEmail;
        var orderWithUser = request with { UserId = userId, UserEmail = userEmail };
        var result = await _mediator.Send(new CreateOrderCommand(orderWithUser));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id)
    {
        var order = await _orderRepository.GetByIdWithItemsAsync(id);
        if (order == null) return NotFound();
        return Ok(new OrderDto(order.Id, order.UserId, order.UserEmail, order.Status.ToString(), order.PaymentStatus.ToString(), order.TotalAmount, order.ShippingAddress, order.Items.Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity)).ToList(), order.CreatedAt));
    }

    [HttpGet("my-orders")]
    public async Task<ActionResult<List<OrderDto>>> GetMyOrders()
    {
        var userId = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var orders = await _orderRepository.GetByUserIdAsync(userId);
        return Ok(orders.Select(o => new OrderDto(o.Id, o.UserId, o.UserEmail, o.Status.ToString(), o.PaymentStatus.ToString(), o.TotalAmount, o.ShippingAddress, o.Items.Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity)).ToList(), o.CreatedAt)).ToList());
    }
}