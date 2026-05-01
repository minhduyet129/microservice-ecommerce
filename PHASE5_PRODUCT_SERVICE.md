# 🎯 GIAI ĐOẠN 5: PRODUCT SERVICE

---

## Bước 5.1: Tạo ProductService.Domain Project

```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce
dotnet new classlib -n ProductService.Domain -o src/services/product/src/ProductService.Domain
rm src/services/product/src/ProductService.Domain/Class1.cs
dotnet sln add src/services/product/src/ProductService.Domain/ProductService.Domain.csproj
```

> **Giải thích:**
> - **Domain layer**: Chứa Product entity, Category entity, IProductRepository

---

## Bước 5.2: Tạo ProductService.Application Project

```bash
dotnet new classlib -n ProductService.Application -o src/services/product/src/ProductService.Application
rm src/services/product/src/ProductService.Application/Class1.cs
dotnet sln add src/services/product/src/ProductService.Application/ProductService.Application.csproj
```

> **Giải thích:**
> - **Application layer**: Chứa CRUD commands/queries cho products

---

## Bước 5.3: Tạo ProductService.Infrastructure Project

```bash
dotnet new classlib -n ProductService.Infrastructure -o src/services/product/src/ProductService.Infrastructure
rm src/services/product/src/ProductService.Infrastructure/Class1.cs
dotnet sln add src/services/product/src/ProductService.Infrastructure/ProductService.Infrastructure.csproj
```

> **Giải thích:**
> - **Infrastructure layer**: Chứa EF Core DbContext và repositories

---

## Bước 5.4: Tạo ProductService.Api Project

```bash
dotnet new webapi -n ProductService.Api -o src/services/product/src/ProductService.Api
dotnet sln add src/services/product/src/ProductService.Api/ProductService.Api.csproj
```

> **Giải thích:**
> - **API layer**: REST API cho product management

---

## Bước 5.5: Tạo ProductService.gRPC Project

```bash
dotnet new grpc -n ProductService.gRPC -o src/services/product/src/ProductService.gRPC --framework net8.0
dotnet sln add src/services/product/src/ProductService.gRPC/ProductService.gRPC.csproj
```

> **Giải thích:**
> - **gRPC project**: Inter-service communication (Order gọi Product qua gRPC)

---

## Bước 5.6: Add Project References

```bash
dotnet add src/services/product/src/ProductService.Application/ProductService.Application.csproj reference src/services/product/src/ProductService.Domain/ProductService.Domain.csproj
dotnet add src/services/product/src/ProductService.Application/ProductService.Application.csproj reference src/buildingblocks/Core/BuildingBlocks.Core.csproj

dotnet add src/services/product/src/ProductService.Infrastructure/ProductService.Infrastructure.csproj reference src/services/product/src/ProductService.Domain/ProductService.Domain.csproj
dotnet add src/services/product/src/ProductService.Infrastructure/ProductService.Infrastructure.csproj reference src/services/product/src/ProductService.Application/ProductService.Application.csproj

dotnet add src/services/product/src/ProductService.Api/ProductService.Api.csproj reference src/services/product/src/ProductService.Application/ProductService.Application.csproj
dotnet add src/services/product/src/ProductService.Api/ProductService.Api.csproj reference src/services/product/src/ProductService.Infrastructure/ProductService.Infrastructure.csproj
dotnet add src/services/product/src/ProductService.Api/ProductService.Api.csproj reference src/services/product/src/ProductService.gRPC/ProductService.gRPC.csproj

dotnet add src/services/product/src/ProductService.gRPC/ProductService.gRPC.csproj reference src/services/product/src/ProductService.Domain/ProductService.Domain.csproj
```

> **Giải thích:**
> - Theo Clean Architecture: Api → Application → Domain
> - gRPC project chỉ cần reference Domain

---

## Bước 5.7: Update .csproj Files

**ProductService.Domain.csproj:**

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

**ProductService.Application.csproj:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\ProductService.Domain\ProductService.Domain.csproj" />
    <ProjectReference Include="..\..\..\..\buildingblocks\Shared\BuildingBlocks.Shared.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="FluentValidation" Version="11.9.0" />
    <PackageReference Include="MediatR" Version="12.2.0" />
  </ItemGroup>
</Project>
```

**ProductService.Infrastructure.csproj:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\ProductService.Domain\ProductService.Domain.csproj" />
    <ProjectReference Include="..\ProductService.Application\ProductService.Application.csproj" />
    <ProjectReference Include="..\..\..\..\buildingblocks\Core\BuildingBlocks.Core.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
  </ItemGroup>
</Project>
```

**ProductService.Api.csproj:**

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
    <PackageReference Include="MediatR" Version="12.2.0" />
    <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\ProductService.Application\ProductService.Application.csproj" />
    <ProjectReference Include="..\ProductService.Infrastructure\ProductService.Infrastructure.csproj" />
    <ProjectReference Include="..\..\..\..\buildingblocks\Core\BuildingBlocks.Core.csproj" />
    <ProjectReference Include="..\..\..\..\buildingblocks\Shared\BuildingBlocks.Shared.csproj" />
    <ProjectReference Include="..\ProductService.gRPC\ProductService.gRPC.csproj" />
  </ItemGroup>
</Project>
```

> **Giải thích:**
> - Tất cả dùng net8.0
> - Infrastructure dùng Npgsql (PostgreSQL)
> - Api cần reference gRPC project để serve gRPC

---

## Bước 5.8: Tạo Domain Entities

**ProductService.Domain/Entities/Category.cs:**

```csharp
using BuildingBlocks.Core.Abstractions;

namespace ProductService.Domain.Entities;

public class Category : Entity<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    public Category()
    {
        Id = Guid.NewGuid();
    }

    public static Category Create(string name, string? description = null, int sortOrder = 0)
    {
        return new Category { Name = name, Description = description, SortOrder = sortOrder, IsActive = true, CreatedAt = DateTime.UtcNow };
    }
}
```

**ProductService.Domain/Entities/Product.cs:**

```csharp
using BuildingBlocks.Core.Abstractions;

namespace ProductService.Domain.Entities;

public class Product : Entity<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public Guid CategoryId { get; set; }
    public bool IsActive { get; set; } = true;
    public string? SKU { get; set; }

    public Product()
    {
        Id = Guid.NewGuid();
    }

    public static Product Create(string name, string? description, decimal price, int stockQuantity, Guid categoryId, string? sku = null)
    {
        return new Product { Name = name, Description = description, Price = price, StockQuantity = stockQuantity, CategoryId = categoryId, SKU = sku ?? $"SKU-{Guid.NewGuid().ToString()[..8].ToUpper()}", IsActive = true, CreatedAt = DateTime.UtcNow };
    }

    public void UpdateStock(int quantity)
    {
        StockQuantity += quantity;
        if (StockQuantity < 0) StockQuantity = 0;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
```

> **Giải thích:**
> - **Product**: Entity chính - có name, price, stock, SKU
> - **Category**: Phân loại sản phẩm
> - **UpdateStock()**: Cập nhật tồn kho (dùng cho giảm stock khi đặt hàng)

---

## Bước 5.9: Tạo IProductRepository

Tạo file `ProductService.Domain/Repositories/IProductRepository.cs`:

```csharp
using ProductService.Domain.Entities;

namespace ProductService.Domain.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default);
    Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> IsInStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default);
}
```

> **Giải thích:**
> - Interface cho product data access
> - **IsInStockAsync()**: Kiểm tra đủ hàng không (dùng khi Order gọi)

---

## Bước 5.10: Tạo Infrastructure (DbContext + Repository)

**ProductService.Infrastructure/Persistence/ProductDbContext.cs:**

```csharp
using ProductService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Infrastructure.Persistence;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.HasIndex(e => e.SKU).IsUnique();
        });
    }
}
```

**ProductService.Infrastructure/Persistence/Repositories/ProductRepository.cs:**

```csharp
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Infrastructure.Persistence.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ProductDbContext _context;

    public ProductRepository(ProductDbContext context) { _context = context; }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) 
        => await _context.Products.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default) 
        => await _context.Products.Where(p => p.IsActive).ToListAsync(ct);

    public async Task<IReadOnlyList<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken ct = default) 
        => await _context.Products.Where(p => p.CategoryId == categoryId && p.IsActive).ToListAsync(ct);

    public async Task<Product> AddAsync(Product product, CancellationToken ct = default)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync(ct);
        return product;
    }

    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _context.Products.FindAsync(new object[] { id }, ct);
        if (product != null) { product.Deactivate(); await _context.SaveChangesAsync(ct); }
    }

    public async Task<bool> IsInStockAsync(Guid productId, int quantity, CancellationToken ct = default)
    {
        var product = await _context.Products.FindAsync(new object[] { productId }, ct);
        return product != null && product.StockQuantity >= quantity;
    }
}
```

> **Giải thích:**
> - **ProductDbContext**: DbContext cho PostgreSQL
> - **ProductRepository**: Implementation của IProductRepository
> - **DeleteAsync**: Soft delete (deactivate) thay vì xóa thật

---

## Bước 5.11: Tạo Application Layer (Commands + Queries)

**ProductService.Application/DTOs/ProductDtos.cs:**

```csharp
namespace ProductService.Application.DTOs;

public record ProductDto(Guid Id, string Name, string? Description, decimal Price, decimal? DiscountPrice, int StockQuantity, string? ImageUrl, Guid CategoryId, string? SKU);
public record CreateProductRequest(string Name, string? Description, decimal Price, int StockQuantity, Guid CategoryId, string? SKU);
public record UpdateProductRequest(Guid Id, string Name, string? Description, decimal Price, int StockQuantity, Guid CategoryId, decimal? DiscountPrice);
```

**ProductService.Application/Commands/ProductCommands.cs:**

```csharp
using FluentValidation;
using MediatR;
using ProductService.Application.DTOs;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Commands;

public record CreateProductCommand(CreateProductRequest Request) : IRequest<ProductDto>;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Price).GreaterThan(0);
        RuleFor(x => x.Request.StockQuantity).GreaterThanOrEqualTo(0);
    }
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductRepository _productRepository;

    public CreateProductCommandHandler(IProductRepository productRepository) { _productRepository = productRepository; }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var product = Domain.Entities.Product.Create(request.Request.Name, request.Request.Description, request.Request.Price, request.Request.StockQuantity, request.Request.CategoryId, request.Request.SKU);
        await _productRepository.AddAsync(product, ct);
        return new ProductDto(product.Id, product.Name, product.Description, product.Price, product.DiscountPrice, product.StockQuantity, product.ImageUrl, product.CategoryId, product.SKU);
    }
}

public record UpdateProductCommand(UpdateProductRequest Request) : IRequest<ProductDto>;
public record DeleteProductCommand(Guid Id) : IRequest<bool>;
```

**ProductService.Application/Queries/ProductQueries.cs:**

```csharp
using MediatR;
using ProductService.Application.DTOs;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Queries;

public record GetAllProductsQuery : IRequest<List<ProductDto>>;

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, List<ProductDto>>
{
    private readonly IProductRepository _productRepository;

    public GetAllProductsQueryHandler(IProductRepository productRepository) { _productRepository = productRepository; }

    public async Task<List<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken ct)
    {
        var products = await _productRepository.GetAllAsync(ct);
        return products.Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Price, p.DiscountPrice, p.StockQuantity, p.ImageUrl, p.CategoryId, p.SKU)).ToList();
    }
}

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto?>;
```

> **Giải thích:**
> - **Commands**: CreateProduct, UpdateProduct, DeleteProduct
> - **Queries**: GetAllProducts, GetProductById
> - Sử dụng CQRS pattern qua MediatR

---

## Bước 5.12: Tạo gRPC Service

**ProductService.gRPC/Protos/product.proto:**

```protobuf
syntax = "proto3";

option csharp_namespace = "ProductService.gRPC";

package ProductService;

service ProductGrpc {
  rpc GetProduct (ProductRequest) returns (ProductResponse);
  rpc CheckStock (StockRequest) returns (StockResponse);
  rpc ReduceStock (StockReductionRequest) returns (StockResponse);
}

message ProductRequest { string product_id = 1; }

message ProductResponse {
  string id = 1; string name = 2; string description = 3;
  double price = 4; double discount_price = 5;
  int32 stock_quantity = 6; string sku = 7; bool is_valid = 8;
}

message StockRequest { string product_id = 1; int32 quantity = 2; }
message StockReductionRequest { string product_id = 1; int32 quantity = 2; }
message StockResponse { bool success = 1; string message = 2; }
```

> **Giải thích:**
> - **Protobuf**: Interface definition language cho gRPC
> - **ProductGrpc service**: 3 methods cho inter-service communication
> - **GetProduct**: Lấy thông tin sản phẩm
> - **CheckStock**: Kiểm tra đủ hàng
> - **ReduceStock**: Giảm tồn kho (khi Order đặt hàng)

**ProductService.gRPC/Services/ProductGrpcService.cs:**

```csharp
using Grpc.Core;
using ProductService.gRPC;
using ProductService.Domain.Repositories;

namespace ProductService.gRPC.Services;

public class ProductGrpcService : ProductGrpc.ProductGrpcBase
{
    private readonly IProductRepository _productRepository;

    public ProductGrpcService(IProductRepository productRepository) { _productRepository = productRepository; }

    public override async Task<ProductResponse> GetProduct(ProductRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ProductId, out var productId)) return new ProductResponse { IsValid = false };
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) return new ProductResponse { IsValid = false };
        return new ProductResponse { Id = product.Id.ToString(), Name = product.Name, Description = product.Description ?? "", Price = (double)product.Price, DiscountPrice = product.DiscountPrice.HasValue ? (double)product.DiscountPrice.Value : 0, StockQuantity = product.StockQuantity, Sku = product.SKU ?? "", IsValid = true };
    }

    public override async Task<StockResponse> CheckStock(StockRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ProductId, out var productId)) return new StockResponse { Success = false, Message = "Invalid product ID" };
        var isInStock = await _productRepository.IsInStockAsync(productId, request.Quantity);
        return new StockResponse { Success = isInStock, Message = isInStock ? "Stock available" : "Insufficient stock" };
    }

    public override async Task<StockResponse> ReduceStock(StockReductionRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ProductId, out var productId)) return new StockResponse { Success = false, Message = "Invalid product ID" };
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) return new StockResponse { Success = false, Message = "Product not found" };
        if (product.StockQuantity < request.Quantity) return new StockResponse { Success = false, Message = "Insufficient stock" };
        product.UpdateStock(-request.Quantity);
        await _productRepository.UpdateAsync(product);
        return new StockResponse { Success = true, Message = "Stock reduced successfully" };
    }
}
```

> **Giải thích:**
> - Implement gRPC methods
> - **ReduceStock**: Dùng khi Order đặt hàng - giảm số lượng tồn kho

---

## Bước 5.13: Tạo API Layer

**ProductService.Api/appsettings.json:**

```json
{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*",
  "ConnectionStrings": { "DefaultConnection": "Host=localhost;Port=5432;Database=productdb;Username=sa;Password=YourStrong!Passw0rd" },
  "Urls": "http://0.0.0.0:5002",
  "Serilog": { "MinimumLevel": { "Default": "Information" }, "WriteTo": [{ "Name": "Console" }] }
}
```

> **Giải thích:**
> - PostgreSQL connection string
> - Port 5002 cho Product Service

**ProductService.Api/Program.cs:**

```csharp
using ProductService.Application.Commands;
using ProductService.Application.Queries;
using ProductService.Infrastructure.Persistence;
using ProductService.Infrastructure.Persistence.Repositories;
using ProductService.Domain.Repositories;
using ProductService.gRPC.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ProductDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly));

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductCommand>();

Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).Enrich.FromLogContext().CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services.AddHealthChecks().AddDbContextCheck<ProductDbContext>("sqlserver");

var app = builder.Build();

if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }

app.UseSerilogRequestLogging();
app.MapControllers();
app.MapGrpcService<ProductGrpcService>();
app.MapHealthChecks("/health");

app.Run();
```

> **Giải thích:**
> - **MapGrpcService()**: Enable gRPC endpoint
> - Kết hợp REST API + gRPC trong cùng service

**ProductService.Api/Controllers/ProductsController.cs:**

```csharp
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Commands;
using ProductService.Application.DTOs;
using ProductService.Application.Queries;

namespace ProductService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator) { _mediator = mediator; }

    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetAll() => Ok(await _mediator.Send(new GetAllProductsQuery()));

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductRequest request)
    {
        var result = await _mediator.Send(new CreateProductCommand(request));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> Delete(Guid id) => Ok(await _mediator.Send(new DeleteProductCommand(id)));
}
```

> **Giải thích:**
> - REST endpoints cho Products
> - MediatR handle business logic

---

## Bước 5.14: Tạo Database và Build

```bash
# Tạo database
docker exec -it ecommerce_postgres psql -U sa -d postgres -c "CREATE DATABASE productdb;"

# Build
dotnet build src/services/product/src/ProductService.Api/ProductService.Api.csproj

# Migration
cd src/services/product/src/ProductService.Api
dotnet ef migrations add InitialCreate --output-dir ../ProductService.Infrastructure/Persistence/Migrations
dotnet ef database update
```

> **Giải thích:**
> - Tạo database riêng cho Product
> - Build và chạy migration để tạo tables

---

## ✅ CHECKLIST GIAI ĐOẠN 5

| Task | Mô tả | Status |
|------|-------|--------|
| 5.1-5.5 | Tạo 5 projects | ⬜ |
| 5.6 | Add references | ⬜ |
| 5.7 | Update .csproj | ⬜ |
| 5.8-5.9 | Domain entities | ⬜ |
| 5.10 | Infrastructure | ⬜ |
| 5.11 | Application layer | ⬜ |
| 5.12 | gRPC service | ⬜ |
| 5.13 | API layer | ⬜ |
| 5.14 | Database & Build | ⬜ |

---

## ❓ KHI HOÀN THÀNH

Reply: **"Done Phase 5"**

Tôi sẽ hướng dẫn tiếp **Giai đoạn 6: Order Service**