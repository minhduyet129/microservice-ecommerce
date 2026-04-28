# 🎯 GIAI ĐOẠN 6: ORDER SERVICE
## Thời gian: Day 22-28
## Mục tiêu: Tạo service xử lý đơn hàng với Saga Pattern + RabbitMQ + Polly

---

## 📝 TASK 6.1: TẠO 4 LAYER PROJECTS

### Bước 6.1.1: Tạo Domain project

```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce

# OrderService.Domain
dotnet new classlib -n OrderService.Domain -o src/services/order/src/OrderService.Domain
rm src/services/order/src/OrderService.Domain/Class1.cs
dotnet sln add src/services/order/src/OrderService.Domain/OrderService.Domain.csproj
```

### Bước 6.1.2: Tạo Application project

```bash
# OrderService.Application
dotnet new classlib -n OrderService.Application -o src/services/order/src/OrderService.Application
rm src/services/order/src/OrderService.Application/Class1.cs
dotnet sln add src/services/order/src/OrderService.Application/OrderService.Application.csproj
```

### Bước 6.1.3: Tạo Infrastructure project

```bash
# OrderService.Infrastructure
dotnet new classlib -n OrderService.Infrastructure -o src/services/order/src/OrderService.Infrastructure
rm src/services/order/src/OrderService.Infrastructure/Class1.cs
dotnet sln add src/services/order/src/OrderService.Infrastructure/OrderService.Infrastructure.csproj
```

### Bước 6.1.4: Tạo Api project

```bash
# OrderService.Api
dotnet new webapi -n OrderService.Api -o src/services/order/src/OrderService.Api
dotnet sln add src/services/order/src/OrderService.Api/OrderService.Api.csproj
```

### Bước 6.1.5: Add project references

```bash
# Application reference Domain + BuildingBlocks
dotnet add src/services/order/src/OrderService.Application/OrderService.Application.csproj reference src/services/order/src/OrderService.Domain/OrderService.Domain.csproj
dotnet add src/services/order/src/OrderService.Application/OrderService.Application.csproj reference src/buildingblocks/Core/BuildingBlocks.Core.csproj
dotnet add src/services/order/src/OrderService.Application/OrderService.Application.csproj reference src/buildingblocks/Shared/BuildingBlocks.Shared.csproj

# Infrastructure reference Domain + Application + Product gRPC
dotnet add src/services/order/src/OrderService.Infrastructure/OrderService.Infrastructure.csproj reference src/services/order/src/OrderService.Domain/OrderService.Domain.csproj
dotnet add src/services/order/src/OrderService.Infrastructure/OrderService.Infrastructure.csproj reference src/services/order/src/OrderService.Application/OrderService.Application.csproj
dotnet add src/services/order/src/OrderService.Infrastructure/OrderService.Infrastructure.csproj reference src/services/product/src/ProductService.gRPC/ProductService.gRPC.csproj
dotnet add src/services/order/src/OrderService.Infrastructure/OrderService.Infrastructure.csproj reference src/buildingblocks/Core/BuildingBlocks.Core.csproj

# Api reference Application + Infrastructure
dotnet add src/services/order/src/OrderService.Api/OrderService.Api.csproj reference src/services/order/src/OrderService.Application/OrderService.Application.csproj
dotnet add src/services/order/src/OrderService.Api/OrderService.Api.csproj reference src/services/order/src/OrderService.Infrastructure/OrderService.Infrastructure.csproj
dotnet add src/services/order/src/OrderService.Api/OrderService.Api.csproj reference src/buildingblocks/Core/BuildingBlocks.Core.csproj
```

---

## 📝 TASK 6.2: SETUP DOMAIN ENTITIES

### Bước 6.2.1: Thêm reference vào Domain

```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce\src\services\order\src\OrderService.Domain
dotnet add reference ../../../../buildingblocks/Core/BuildingBlocks.Core.csproj
```

### Bước 6.2.2: Tạo Enums

Tạo file `src/services/order/src/OrderService.Domain/Enums/OrderStatus.cs`:

```csharp
namespace OrderService.Domain.Enums;

public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Processing = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5
}

public enum PaymentStatus
{
    Pending = 0,
    Paid = 1,
    Failed = 2,
    Refunded = 3
}
```

### Bước 6.2.3: Tạo OrderItem entity

Tạo file `src/services/order/src/OrderService.Domain/Entities/OrderItem.cs`:

```csharp
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
        return new OrderItem
        {
            OrderId = orderId,
            ProductId = productId,
            ProductName = productName,
            UnitPrice = unitPrice,
            Quantity = quantity,
            CreatedAt = DateTime.UtcNow
        };
    }
}
```

### Bước 6.2.4: Tạo Order aggregate root

Tạo file `src/services/order/src/OrderService.Domain/Entities/Order.cs`:

```csharp
using BuildingBlocks.Core.Abstractions;
using BuildingBlocks.Core.Events;
using OrderService.Domain.Enums;

namespace OrderService.Domain.Entities;

public class Order : IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

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

    public Order()
    {
        Id = Guid.NewGuid();
    }

    public static Order Create(string userId, string userEmail, string shippingAddress, string shippingPhone)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UserEmail = userEmail,
            ShippingAddress = shippingAddress,
            ShippingPhone = shippingPhone,
            Status = OrderStatus.Pending,
            PaymentStatus = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        order.AddDomainEvent(new OrderCreatedEvent(order.Id, order.UserId, order.TotalAmount));

        return order;
    }

    public void AddItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        var orderItem = OrderItem.Create(Id, productId, productName, unitPrice, quantity);
        Items.Add(orderItem);
        RecalculateTotal();
    }

    public void RecalculateTotal()
    {
        TotalAmount = Items.Sum(i => i.TotalPrice);
    }

    public void Confirm()
    {
        Status = OrderStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new OrderConfirmedEvent(Id));
    }

    public void Cancel(string reason)
    {
        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new OrderCancelledEvent(Id, reason));
    }

    public void MarkAsPaid()
    {
        PaymentStatus = PaymentStatus.Paid;
        Status = OrderStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}

// Domain Events
public record OrderCreatedEvent(Guid OrderId, string UserId, decimal TotalAmount) : IDomainEvent;
public record OrderConfirmedEvent(Guid OrderId) : IDomainEvent;
public record OrderCancelledEvent(Guid OrderId, string Reason) : IDomainEvent;
```

### Bước 6.2.5: Tạo IOrderRepository

Tạo file `src/services/order/src/OrderService.Domain/Repositories/IOrderRepository.cs`:

```csharp
using OrderService.Domain.Entities;

namespace OrderService.Domain.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default);
    Task UpdateAsync(Order order, CancellationToken cancellationToken = default);
}
```

---

## 📝 TASK 6.3: SETUP INFRASTRUCTURE

### Bước 6.3.1: Thêm packages

```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce\src\services\order\src\OrderService.Infrastructure

dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package MassTransit.RabbitMQ
dotnet add package Polly
dotnet add package Grpc.Net.Client
```

### Bước 6.3.2: Tạo OrderDbContext

Tạo file `src/services/order/src/OrderService.Infrastructure/Persistence/OrderDbContext.cs`:

```csharp
using OrderService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Infrastructure.Persistence;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UserEmail).HasMaxLength(255).IsRequired();
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ShippingAddress).HasMaxLength(500);
            entity.Property(e => e.ShippingPhone).HasMaxLength(20);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItems");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.HasIndex(e => e.OrderId);
        });
    }
}
```

### Bước 6.3.3: Tạo OrderRepository

Tạo file `src/services/order/src/OrderService.Infrastructure/Persistence/Repositories/OrderRepository.cs`:

```csharp
using OrderService.Domain.Entities;
using OrderService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Infrastructure.Persistence.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;

    public OrderRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);
        return order;
    }

    public async Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
```

---

## 📝 TASK 6.4: IMPLEMENT APPLICATION LAYER

### Bước 6.4.1: Thêm packages

```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce\src\services\order\src\OrderService.Application
dotnet add package FluentValidation
dotnet add package MediatR
```

### Bước 6.4.2: Tạo DTOs

Tạo file `src/services/order/src/OrderService.Application/DTOs/OrderDtos.cs`:

```csharp
namespace OrderService.Application.DTOs;

public record OrderItemDto(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);
public record CreateOrderRequest(string UserId, string UserEmail, string ShippingAddress, string ShippingPhone, List<OrderItemDto> Items);
public record OrderDto(Guid Id, string UserId, string UserEmail, string Status, string PaymentStatus, decimal TotalAmount, string? ShippingAddress, List<OrderItemDto> Items, DateTime CreatedAt);
```

### Bước 6.4.3: Tạo ProductGrpcClient

Tạo file `src/services/order/src/OrderService.Application/Services/ProductGrpcClient.cs`:

```csharp
using ProductService.gRPC;

namespace OrderService.Application.Services;

public interface IProductGrpcClient
{
    Task<ProductResponse?> GetProductAsync(Guid productId);
    Task<bool> ReduceStockAsync(Guid productId, int quantity);
}

public class ProductGrpcClient : IProductGrpcClient
{
    private readonly ProductGrpc.ProductGrpcClient _client;

    public ProductGrpcClient(ProductGrpc.ProductGrpcClient client)
    {
        _client = client;
    }

    public async Task<ProductResponse?> GetProductAsync(Guid productId)
    {
        try
        {
            var response = await _client.GetProductAsync(new ProductRequest { ProductId = productId.ToString() });
            return response.IsValid ? response : null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> ReduceStockAsync(Guid productId, int quantity)
    {
        try
        {
            var response = await _client.ReduceStockAsync(new StockReductionRequest
            {
                ProductId = productId.ToString(),
                Quantity = quantity
            });
            return response.Success;
        }
        catch
        {
            return false;
        }
    }
}
```

### Bước 6.4.4: Tạo CreateOrderCommand

Tạo file `src/services/order/src/OrderService.Application/Commands/CreateOrderCommand.cs`:

```csharp
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

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = Order.Create(
            request.Request.UserId,
            request.Request.UserEmail,
            request.Request.ShippingAddress,
            request.Request.ShippingPhone
        );

        foreach (var item in request.Request.Items)
        {
            var product = await _productClient.GetProductAsync(item.ProductId);
            if (product == null)
                throw new Exception($"Product {item.ProductId} not found");

            order.AddItem(item.ProductId, product.Name, (decimal)product.Price, item.Quantity);
        }

        var createdOrder = await _orderRepository.AddAsync(order, cancellationToken);

        // Try to reduce stock
        foreach (var item in request.Request.Items)
        {
            await _productClient.ReduceStockAsync(item.ProductId, item.Quantity);
        }

        return new OrderDto(
            createdOrder.Id,
            createdOrder.UserId,
            createdOrder.UserEmail,
            createdOrder.Status.ToString(),
            createdOrder.PaymentStatus.ToString(),
            createdOrder.TotalAmount,
            createdOrder.ShippingAddress,
            createdOrder.Items.Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity)).ToList(),
            createdOrder.CreatedAt
        );
    }
}
```

---

## 📝 TASK 6.5: SETUP RABBITMQ MESSAGING

### Bước 6.5.1: Tạo Event Messages

Tạo file `src/services/order/src/OrderService.Domain/Events/OrderEvents.cs`:

```csharp
namespace OrderService.Domain.Events;

public record OrderCreatedMessage(Guid OrderId, string UserId, decimal TotalAmount, DateTime CreatedAt);
public record OrderConfirmedMessage(Guid OrderId, DateTime ConfirmedAt);
public record OrderCancelledMessage(Guid OrderId, string Reason, DateTime CancelledAt);
```

### Bước 6.5.2: Tạo Event Publisher

Tạo file `src/services/order/src/OrderService.Infrastructure/Events/OrderEventPublisher.cs`:

```csharp
using OrderService.Domain.Events;
using MassTransit;

namespace OrderService.Infrastructure.Events;

public interface IOrderEventPublisher
{
    Task PublishOrderCreatedAsync(OrderCreatedMessage message);
    Task PublishOrderConfirmedAsync(OrderConfirmedMessage message);
    Task PublishOrderCancelledAsync(OrderCancelledMessage message);
}

public class OrderEventPublisher : IOrderEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public OrderEventPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishOrderCreatedAsync(OrderCreatedMessage message)
    {
        await _publishEndpoint.Publish(message);
    }

    public async Task PublishOrderConfirmedAsync(OrderConfirmedMessage message)
    {
        await _publishEndpoint.Publish(message);
    }

    public async Task PublishOrderCancelledAsync(OrderCancelledMessage message)
    {
        await _publishEndpoint.Publish(message);
    }
}
```

---

## 📝 TASK 6.6: SETUP POLLY CIRCUIT BREAKER

### Bước 6.6.1: Tạo ResiliencePolicies

Tạo file `src/services/order/src/OrderService.Infrastructure/Resilience/ResiliencePolicies.cs`:

```csharp
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Wrap;

namespace OrderService.Infrastructure.Resilience;

public static class ResiliencePolicies
{
    public static AsyncPolicyWrap<HttpResponseMessage> GetHttpPolicy()
    {
        var retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        var circuitBreakerPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

        return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);
    }

    public static AsyncPolicy GetGrpcPolicy()
    {
        return Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
            .WrapAsync(
                Policy
                    .Handle<Exception>()
                    .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30))
            );
    }
}
```

---

## 📝 TASK 6.7: SETUP API LAYER

### Bước 6.7.1: Thêm packages

```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce\src\services\order\src\OrderService.Api
dotnet add package FluentValidation.AspNetCore
dotnet add package MediatR
dotnet add package Serilog.AspNetCore
dotnet add package MassTransit.AspNetCore
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

### Bước 6.7.2: Update Program.cs

Tạo file `src/services/order/src/OrderService.Api/Program.cs`:

```csharp
using MassTransit;
using OrderService.Application.Commands;
using OrderService.Application.DTOs;
using OrderService.Application.Services;
using OrderService.Infrastructure.Events;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Persistence.Repositories;
using OrderService.Domain.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using ProductService.gRPC;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly));

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderCommand>();

// gRPC Client
builder.Services.AddGrpcClient<ProductGrpc.ProductGrpcClient>(options =>
{
    options.Address = new Uri(builder.Configuration["GrpcSettings:ProductServiceUrl"]!);
});

// MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]!);
            h.Password(builder.Configuration["RabbitMQ:Password"]!);
        });
    });
});

// Register services
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IProductGrpcClient, ProductGrpcClient>();
builder.Services.AddScoped<IOrderEventPublisher, OrderEventPublisher>();

// JWT Authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
        };
    });

builder.Services.AddAuthorization();

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<OrderDbContext>("sqlserver");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
```

### Bước 6.7.3: Tạo appsettings.json

Tạo file `src/services/order/src/OrderService.Api/appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=OrderDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True"
  },
  "Urls": "http://0.0.0.0:5003",
  "Jwt": {
    "Secret": "ThisIsASecretKeyForJwtTokenGeneration123456",
    "Issuer": "MicroserviceEcommerce",
    "Audience": "MicroserviceEcommerce"
  },
  "GrpcSettings": {
    "ProductServiceUrl": "http://localhost:5002"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" }
    ]
  }
}
```

### Bước 6.7.4: Tạo OrdersController

Tạo file `src/services/order/src/OrderService.Api/Controllers/OrdersController.cs`:

```csharp
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

    public OrdersController(IMediator mediator, IOrderRepository orderRepository)
    {
        _mediator = mediator;
        _orderRepository = orderRepository;
    }

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

        return Ok(new OrderDto(
            order.Id, order.UserId, order.UserEmail,
            order.Status.ToString(), order.PaymentStatus.ToString(),
            order.TotalAmount, order.ShippingAddress,
            order.Items.Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity)).ToList(),
            order.CreatedAt
        ));
    }

    [HttpGet("my-orders")]
    public async Task<ActionResult<List<OrderDto>>> GetMyOrders()
    {
        var userId = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var orders = await _orderRepository.GetByUserIdAsync(userId);

        return Ok(orders.Select(o => new OrderDto(
            o.Id, o.UserId, o.UserEmail,
            o.Status.ToString(), o.PaymentStatus.ToString(),
            o.TotalAmount, o.ShippingAddress,
            o.Items.Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity)).ToList(),
            o.CreatedAt
        )).ToList());
    }
}
```

---

## 📝 TASK 6.8: CREATE MIGRATION VÀ BUILD

### Bước 6.8.1: Create migration

```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce\src\services\order\src\OrderService.Api
dotnet ef migrations add InitialCreate --output-dir ../OrderService.Infrastructure/Persistence/Migrations
```

### Bước 6.8.2: Build solution

```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce
dotnet build
```

---

## ✅ CHECKLIST GIAI ĐOẠN 6

| Task | Status | Ghi chú |
|------|--------|---------|
| 6.1.1 | ⬜ | Tạo Domain project |
| 6.1.2 | ⬜ | Tạo Application project |
| 6.1.3 | ⬜ | Tạo Infrastructure project |
| 6.1.4 | ⬜ | Tạo Api project |
| 6.1.5 | ⬜ | Add project references |
| 6.2.1 | ⬜ | Add reference vào Domain |
| 6.2.2 | ⬜ | Tạo Enums |
| 6.2.3 | ⬜ | Tạo OrderItem entity |
| 6.2.4 | ⬜ | Tạo Order aggregate root |
| 6.2.5 | ⬜ | Tạo IOrderRepository |
| 6.3.1 | ⬜ | Thêm packages Infrastructure |
| 6.3.2 | ⬜ | Tạo OrderDbContext |
| 6.3.3 | ⬜ | Tạo OrderRepository |
| 6.4.1 | ⬜ | Thêm packages Application |
| 6.4.2 | ⬜ | Tạo DTOs |
| 6.4.3 | ⬜ | Tạo ProductGrpcClient |
| 6.4.4 | ⬜ | Tạo CreateOrderCommand |
| 6.5.1 | ⬜ | Tạo Event Messages |
| 6.5.2 | ⬜ | Tạo Event Publisher |
| 6.6.1 | ⬜ | Tạo ResiliencePolicies |
| 6.7.1 | ⬜ | Thêm packages Api |
| 6.7.2 | ⬜ | Update Program.cs |
| 6.7.3 | ⬜ | Tạo appsettings.json |
| 6.7.4 | ⬜ | Tạo OrdersController |
| 6.8.1 | ⬜ | Create migration |
| 6.8.2 | ⬜ | Build solution |

---

## ❓ KHI HOÀN THÀNH

Khi tất cả tasks hoàn thành, reply:
> **"Done Phase 6"**

Tôi sẽ hướng dẫn tiếp **Giai đoạn 7: Dockerization & CI/CD**