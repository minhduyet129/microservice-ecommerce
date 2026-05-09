# 🎯 GIAI ĐOẠN 6: ORDER SERVICE (VỚI SAGA PATTERN)

---

## Bước 6.1: Tạo OrderService.Domain Project

```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce
dotnet new classlib -n OrderService.Domain -o src/services/order/src/OrderService.Domain
rm src/services/order/src/OrderService.Domain/Class1.cs
dotnet sln add src/services/order/src/OrderService.Domain/OrderService.Domain.csproj
```

> **Giải thích:**
> - **Domain layer**: Chứa Order aggregate root, OrderItem entity, domain events

---

## Bước 6.2: Tạo OrderService.Application Project

```bash
dotnet new classlib -n OrderService.Application -o src/services/order/src/OrderService.Application
rm src/services/order/src/OrderService.Application/Class1.cs
dotnet sln add src/services/order/src/OrderService.Application/OrderService.Application.csproj
```

> **Giải thích:**
> - **Product gRPC reference**: Để gọi Product Service từ Order Service
> - **ProductService.gRPC.csproj**: Cần dùng `GrpcServices="Both"` để generate cả Server và Client code

---

## Bước 6.3: Tạo OrderService.Infrastructure Project

```bash
dotnet new classlib -n OrderService.Infrastructure -o src/services/order/src/OrderService.Infrastructure
rm src/services/order/src/OrderService.Infrastructure/Class1.cs
dotnet sln add src/services/order/src/OrderService.Infrastructure/OrderService.Infrastructure.csproj
```

> **Giải thích:**
> - **Infrastructure layer**: Chứa EF Core DbContext và repositories

---

## Bước 6.4: Tạo OrderService.Api Project

```bash
dotnet new webapi -n OrderService.Api -o src/services/order/src/OrderService.Api
dotnet sln add src/services/order/src/OrderService.Api/OrderService.Api.csproj
```

> **Giải thích:**
> - **API layer**: REST API cho order management

---

## Bước 6.5: Add Project References

```bash
dotnet add src/services/order/src/OrderService.Application/OrderService.Application.csproj reference src/services/order/src/OrderService.Domain/OrderService.Domain.csproj
dotnet add src/services/order/src/OrderService.Application/OrderService.Application.csproj reference src/buildingblocks/Core/BuildingBlocks.Core.csproj
dotnet add src/services/order/src/OrderService.Application/OrderService.Application.csproj reference src/services/product/src/ProductService.gRPC/ProductService.gRPC.csproj

dotnet add src/services/order/src/OrderService.Infrastructure/OrderService.Infrastructure.csproj reference src/services/order/src/OrderService.Domain/OrderService.Domain.csproj
dotnet add src/services/order/src/OrderService.Infrastructure/OrderService.Infrastructure.csproj reference src/services/order/src/OrderService.Application/OrderService.Application.csproj
dotnet add src/services/order/src/OrderService.Infrastructure/OrderService.Infrastructure.csproj reference src/buildingblocks/Core/BuildingBlocks.Core.csproj

dotnet add src/services/order/src/OrderService.Api/OrderService.Api.csproj reference src/services/order/src/OrderService.Application/OrderService.Application.csproj
dotnet add src/services/order/src/OrderService.Api/OrderService.Api.csproj reference src/services/order/src/OrderService.Infrastructure/OrderService.Infrastructure.csproj
```

> **Giải thích:**
> - Theo Clean Architecture: Api → Application → Domain
> - Application cần reference ProductService.gRPC để gọi Product Service

---

## Bước 6.6: Update .csproj Files

**OrderService.Domain.csproj:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\..\..\buildingblocks\Core\BuildingBlocks.Core.csproj" />
  </ItemGroup>
</Project>
```

**OrderService.Application.csproj:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\OrderService.Domain\OrderService.Domain.csproj" />
    <ProjectReference Include="..\..\..\..\buildingblocks\Shared\BuildingBlocks.Shared.csproj" />
    <ProjectReference Include="..\..\..\product\src\ProductService.gRPC\ProductService.gRPC.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="FluentValidation" Version="11.9.0" />
    <PackageReference Include="MediatR" Version="12.2.0" />
  </ItemGroup>
</Project>
```

**OrderService.Infrastructure.csproj:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\OrderService.Domain\OrderService.Domain.csproj" />
    <ProjectReference Include="..\OrderService.Application\OrderService.Application.csproj" />
    <ProjectReference Include="..\..\..\..\buildingblocks\Core\BuildingBlocks.Core.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
  </ItemGroup>
</Project>
```

> **⚠️ QUAN TRỌNG:**
> - Phải có `Microsoft.EntityFrameworkCore.Design` để chạy `dotnet ef migrations`
> - Thêm reference đến ProductService.gRPC để dùng gRPC client

**OrderService.Api.csproj:**

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
    <PackageReference Include="MediatR" Version="12.2.0" />
    <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
    <PackageReference Include="AspNetCore.HealthChecks.NpgSql" Version="8.0.2" />
    <PackageReference Include="Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore" Version="8.0.10" />
    <PackageReference Include="MassTransit.AspNetCore" Version="8.0.16" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\OrderService.Application\OrderService.Application.csproj" />
    <ProjectReference Include="..\OrderService.Infrastructure\OrderService.Infrastructure.csproj" />
    <ProjectReference Include="..\..\..\..\buildingblocks\Core\BuildingBlocks.Core.csproj" />
    <ProjectReference Include="..\..\..\..\buildingblocks\Shared\BuildingBlocks.Shared.csproj" />
  </ItemGroup>
</Project>
```

> **Giải thích:**
> - MassTransit cho message broker (RabbitMQ)
> - JwtBearer cho authentication
> - Thêm `ImplicitUsings` để tránh lỗi CS0246

---

## Bước 6.7: Tạo Domain Entities

**OrderService.Domain/Enums/OrderStatus.cs:**

```csharp
namespace OrderService.Domain.Enums;

public enum OrderStatus { Pending = 0, Confirmed = 1, Processing = 2, Shipped = 3, Delivered = 4, Cancelled = 5 }
public enum PaymentStatus { Pending = 0, Paid = 1, Failed = 2, Refunded = 3 }
```

**OrderService.Domain/Entities/OrderItem.cs:**

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
        return new OrderItem { OrderId = orderId, ProductId = productId, ProductName = productName, UnitPrice = unitPrice, Quantity = quantity, CreatedAt = DateTime.UtcNow };
    }
}
```

> **Giải thích:**
> - **OrderItem**: Mỗi item trong đơn hàng

**OrderService.Domain/Entities/Order.cs:**

```csharp
using BuildingBlocks.Core.Abstractions;
using BuildingBlocks.Core.Events;
using OrderService.Domain.Enums;

namespace OrderService.Domain.Entities;

public class Order : IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // QUAN TRỌNG: Implement IAggregateRoot interface
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

// QUAN TRỌNG: Domain Events PHẢI implement IDomainEvent với OccurredOn
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
```

> **⚠️ Lưu ý quan trọng:**
> - **Order là Aggregate Root**: Tất cả thay đổi phải qua Order
> - **Domain Events**: Khi có thay đổi, tạo events để notify services khác (Saga pattern)
> - **AddItem**: Thêm sản phẩm và tự tính tổng tiền
> - **GetDomainEvents() + AddDomainEvent()**: PHẢI implement để satisfy IAggregateRoot
> - **OccurredOn**: Domain events PHẢI có property này để satisfy IDomainEvent

---

## Bước 6.8: Tạo IOrderRepository

Tạo file `OrderService.Domain/Repositories/IOrderRepository.cs`:

```csharp
using OrderService.Domain.Entities;

namespace OrderService.Domain.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<Order> AddAsync(Order order, CancellationToken ct = default);
    Task UpdateAsync(Order order, CancellationToken ct = default);
}
```

> **Giải thích:**
> - Interface cho Order data access

---

## Bước 6.9: Tạo Infrastructure Layer

**OrderService.Infrastructure/Persistence/OrderDbContext.cs:**

```csharp
using OrderService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Infrastructure.Persistence;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            entity.HasIndex(e => e.UserId);
        });
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("order_items");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
        });
    }
}
```

**OrderService.Infrastructure/Persistence/Repositories/OrderRepository.cs:**

```csharp
using OrderService.Domain.Entities;
using OrderService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Infrastructure.Persistence.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;

    public OrderRepository(OrderDbContext context) { _context = context; }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default) 
        => await _context.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default) 
        => await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<IReadOnlyList<Order>> GetByUserIdAsync(string userId, CancellationToken ct = default) 
        => await _context.Orders.Include(o => o.Items).Where(o => o.UserId == userId).OrderByDescending(o => o.CreatedAt).ToListAsync(ct);

    public async Task<Order> AddAsync(Order order, CancellationToken ct = default)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(ct);
        return order;
    }

    public async Task UpdateAsync(Order order, CancellationToken ct = default)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync(ct);
    }
}
```

> **Giải thích:**
> - EF Core với PostgreSQL
> - **Include(o => o.Items)**: Eager load order items

---

## Bước 6.10: Tạo Application Layer

**OrderService.Application/DTOs/OrderDtos.cs:**

```csharp
namespace OrderService.Application.DTOs;

public record OrderItemDto(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);
public record CreateOrderRequest(string UserId, string UserEmail, string ShippingAddress, string ShippingPhone, List<OrderItemDto> Items);
public record OrderDto(Guid Id, string UserId, string UserEmail, string Status, string PaymentStatus, decimal TotalAmount, string? ShippingAddress, List<OrderItemDto> Items, DateTime CreatedAt);
```

> **Giải thích:**
> - DTOs cho Order creation và response

**OrderService.Application/Services/ProductGrpcClient.cs:**

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

    public ProductGrpcClient(ProductGrpc.ProductGrpcClient client) { _client = client; }

    public async Task<ProductResponse?> GetProductAsync(Guid productId)
    {
        try
        {
            var response = await _client.GetProductAsync(new ProductRequest { ProductId = productId.ToString() });
            return response.IsValid ? response : null;
        }
        catch { return null; }
    }

    public async Task<bool> ReduceStockAsync(Guid productId, int quantity)
    {
        try
        {
            var response = await _client.ReduceStockAsync(new StockReductionRequest { ProductId = productId.ToString(), Quantity = quantity });
            return response.Success;
        }
        catch { return false; }
    }
}
```

> **Giải thích:**
> - Wrapper cho gRPC call đến Product Service
> - Try-catch để handle failures gracefully

**OrderService.Application/Commands/CreateOrderCommand.cs:**

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
```

> **Giải thích:**
> - **Saga Pattern trong command này**:
>   1. Lấy thông tin sản phẩm từ Product Service (gRPC)
>   2. Tạo Order với items
>   3. Lưu Order vào database
>   4. Giảm stock trong Product Service (gRPC)
> - Nếu bước nào fail thì order đã lưu (cần implement compensation)

---

## Bước 6.11: Tạo API Layer

**OrderService.Api/appsettings.json:**

```json
{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*",
  "ConnectionStrings": { "DefaultConnection": "Host=localhost;Port=5432;Database=orderdb;Username=sa;Password=YourStrong!Passw0rd" },
  "Urls": "http://0.0.0.0:5003",
  "Jwt": { "Secret": "ThisIsASecretKeyForJwtTokenGeneration123456", "Issuer": "MicroserviceEcommerce", "Audience": "MicroserviceEcommerce" },
  "GrpcSettings": { "ProductServiceUrl": "http://localhost:5002" },
  "RabbitMQ": { "Host": "localhost", "Username": "guest", "Password": "guest" },
  "Serilog": { "MinimumLevel": { "Default": "Information" }, "WriteTo": [{ "Name": "Console" }] }
}
```

> **Giải thích:**
> - Database: orderdb
> - Port: 5003
> - gRPC Product Service URL
> - RabbitMQ settings cho MassTransit

**OrderService.Api/Program.cs:**

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using OrderService.Application.Commands;
using OrderService.Application.DTOs;
using OrderService.Application.Services;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Persistence.Repositories;
using OrderService.Domain.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using ProductService.gRPC;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OrderDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly));

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderCommand>();

builder.Services.AddGrpcClient<ProductGrpc.ProductGrpcClient>(options => { options.Address = new Uri(builder.Configuration["GrpcSettings:ProductServiceUrl"]!); });

// MassTransit commented out do NuGet package không có version stable
// builder.Services.AddMassTransit(x =>
// {
//     x.UsingRabbitMq((context, cfg) => { cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h => { h.Username(builder.Configuration["RabbitMQ:Username"]!); h.Password(builder.Configuration["RabbitMQ:Password"]!); }); });
// });

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IProductGrpcClient, ProductGrpcClient>();

builder.Services.AddAuthentication("Bearer").AddJwtBearer("Bearer", options =>
{
    options.TokenValidationParameters = new() { ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true, ValidIssuer = builder.Configuration["Jwt:Issuer"], ValidAudience = builder.Configuration["Jwt:Audience"], IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)) };
});

builder.Services.AddAuthorization();

Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).Enrich.FromLogContext().CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddHealthChecks().AddDbContextCheck<OrderDbContext>("orderdb");

var app = builder.Build();

if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }

app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
```

> **⚠️ Lưu ý quan trọng:**
> - **AddGrpcClient**: Đăng ký gRPC client để gọi Product Service
> - **MassTransit**: Commented do NuGet package version không tồn tại
> - **JwtBearer**: Validate JWT tokens - cần thêm using System.IdentityModel.Tokens.Jwt và System.Text
> - **Health check name**: Đổi từ "sqlserver" thành "orderdb"

**OrderService.Api/Controllers/OrdersController.cs:**

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
```

> **Giải thích:**
> - **[Authorize]**: Yêu cầu đăng nhập
> - **GetMyOrders**: Lấy orders của user hiện tại (từ JWT claims)

---

## Bước 6.12: Tạo Database và Build

```bash
# Tạo database
docker exec ecommerce_postgres psql -U sa -d postgres -c "CREATE DATABASE orderdb;"

# Build
dotnet build src/services/order/src/OrderService.Api/OrderService.Api.csproj
```

> **Giải thích:**
> - Tạo database riêng cho Order Service

> **⚠️ QUAN TRỌNG: DbContext nằm trong Infrastructure, không phải Api**

```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce

# Tạo migration (--project = Infrastructure chứa DbContext, --startup-project = Api chứa connection string)
dotnet ef migrations add InitialCreate --project src/services/order/src/OrderService.Infrastructure --startup-project src/services/order/src/OrderService.Api --output-dir Persistence/Migrations

# Apply migration
dotnet ef database update --project src/services/order/src/OrderService.Infrastructure --startup-project src/services/order/src/OrderService.Api

# Verify tables đã tạo
docker exec ecommerce_postgres psql -U sa -d orderdb -c "\dt"
```

> **Giải thích:**
> - DbContext nằm trong `OrderService.Infrastructure/Persistence/OrderDbContext.cs`
> - Migrations output: `OrderService.Infrastructure/Persistence/Migrations/`
> - `--project`: Project chứa DbContext (Infrastructure)
> - `--startup-project`: Project chứa appsettings.json với connection string (Api)

---

## ✅ CHECKLIST GIAI ĐOẠN 6

| Task | Mô tả | Status |
|------|-------|--------|
| 6.1-6.4 | Tạo 4 projects | ⬜ |
| 6.5 | Add references | ⬜ |
| 6.6 | Update .csproj | ⬜ |
| 6.7-6.8 | Domain layer | ⬜ |
| 6.9 | Infrastructure layer | ⬜ |
| 6.10 | Application layer | ⬜ |
| 6.11 | API layer | ⬜ |
| 6.12 | Database & Build | ⬜ |

---

## ❓ KHI HOÀN THÀNH

Reply: **"Done Phase 6"**

Tôi sẽ hướng dẫn tiếp **Giai đoạn 7: Docker + CI/CD**

---

## ⚠️ KHẮC PHỤC LỖI THƯỜNG GẶP

### Lỗi 1: NU1103 - MassTransit.AspNetCore not found
**Nguyên nhân:** NuGet không có version stable 8.x (chỉ có prerelease 9.x)

**Cách fix:**
```xml
<!-- Comment out trong .csproj -->
<!-- PackageReference Include="MassTransit.AspNetCore" Version="8.0.0" /-->
```

### Lỗi 2: NU1605 - Grpc.Net.Client version conflict
**Nguyên nhân:** Infrastructure dùng Grpc.Net.Client 2.0.0 trong khi ProductService.gRPC dùng 2.57.0

**Cách fix:**
```xml
<!-- OrderService.Infrastructure.csproj -->
<PackageReference Include="Grpc.Net.Client" Version="2.57.0" />
```

### Lỗi 3: CS0535 - Order not implement IAggregateRoot.GetDomainEvents()
**Nguyên nhân:** Thiếu method GetDomainEvents() trong Order class

**Cách fix:**
```csharp
public IReadOnlyList<IDomainEvent> GetDomainEvents() => _domainEvents.AsReadOnly();
public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
```

### Lỗi 4: CS0535 - Domain events not implement IDomainEvent.OccurredOn
**Nguyên nhân:** Domain events thiếu property OccurredOn

**Cách fix:**
```csharp
public record OrderCreatedEvent(Guid OrderId, string UserId, decimal TotalAmount) : IDomainEvent
{
    public DateTime OccurredOn => DateTime.UtcNow;
}
```

### Lỗi 5: CS0426 - ProductGrpcClient not found
**Nguyên nhân:** 
1. ProductService.gRPC chỉ generate Server code (GrpcServices="Server")
2. Application project không reference ProductService.gRPC

**Cách fix:**
```xml
<!-- ProductService.gRPC.csproj -->
<Protobuf Include="Protos\product.proto" GrpcServices="Both" />

<!-- OrderService.Application.csproj -->
<ProjectReference Include="..\..\..\product\src\ProductService.gRPC\ProductService.gRPC.csproj" />
```

### Lỗi 6: CS0246 - Task, List, Guid not found trong Controller
**Nguyên nhân:** Thiếu ImplicitUsings trong Api project

**Cách fix:**
```xml
<!-- OrderService.Api.csproj -->
<PropertyGroup>
  <TargetFramework>net8.0</TargetFramework>
  <Nullable>enable</Nullable>
  <ImplicitUsings>enable</ImplicitUsings>
</PropertyGroup>
```

### Lỗi 7: CS0246 - SymmetricSecurityKey, Encoding not found
**Nguyên nhân:** Thiếu using statements trong Program.cs

**Cách fix:**
```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
```

### Build command
```bash
# Build toàn bộ Order Service
dotnet build src/services/order/src/OrderService.Api
```