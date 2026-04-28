# 🎯 GIAI ĐOẠN 5: PRODUCT SERVICE
## Thời gian: Day 16-21
## Mục tiêu: Tạo service quản lý sản phẩm với CRUD + gRPC cho inter-service

---

## 📝 TASK 5.1: TẠO 4 LAYER PROJECTS

### Bước 5.1.1: Tạo Domain project

```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce

# ProductService.Domain
dotnet new classlib -n ProductService.Domain -o src/services/product/src/ProductService.Domain
rm src/services/product/src/ProductService.Domain/Class1.cs
dotnet sln add src/services/product/src/ProductService.Domain/ProductService.Domain.csproj
```

### Bước 5.1.2: Tạo Application project

```bash
# ProductService.Application
dotnet new classlib -n ProductService.Application -o src/services/product/src/ProductService.Application
rm src/services/product/src/ProductService.Application/Class1.cs
dotnet sln add src/services/product/src/ProductService.Application/ProductService.Application.csproj
```

### Bước 5.1.3: Tạo Infrastructure project

```bash
# ProductService.Infrastructure
dotnet new classlib -n ProductService.Infrastructure -o src/services/product/src/ProductService.Infrastructure
rm src/services/product/src/ProductService.Infrastructure/Class1.cs
dotnet sln add src/services/product/src/ProductService.Infrastructure/ProductService.Infrastructure.csproj
```

### Bước 5.1.4: Tạo Api project

```bash
# ProductService.Api
dotnet new webapi -n ProductService.Api -o src/services/product/src/ProductService.Api
dotnet sln add src/services/product/src/ProductService.Api/ProductService.Api.csproj
```

### Bước 5.1.5: Tạo gRPC project

```bash
# ProductService.gRPC
dotnet new grpc -n ProductService.gRPC -o src/services/product/src/ProductService.gRPC --framework net8.0
dotnet sln add src/services/product/src/ProductService.gRPC/ProductService.gRPC.csproj
```

### Bước 5.1.6: Add project references

```bash
# Application reference Domain
dotnet add src/services/product/src/ProductService.Application/ProductService.Application.csproj reference src/services/product/src/ProductService.Domain/ProductService.Domain.csproj
dotnet add src/services/product/src/ProductService.Application/ProductService.Application.csproj reference src/buildingblocks/Core/BuildingBlocks.Core.csproj
dotnet add src/services/product/src/ProductService.Application/ProductService.Application.csproj reference src/buildingblocks/Shared/BuildingBlocks.Shared.csproj

# Infrastructure reference Domain + Application
dotnet add src/services/product/src/ProductService.Infrastructure/ProductService.Infrastructure.csproj reference src/services/product/src/ProductService.Domain/ProductService.Domain.csproj
dotnet add src/services/product/src/ProductService.Infrastructure/ProductService.Infrastructure.csproj reference src/services/product/src/ProductService.Application/ProductService.Application.csproj
dotnet add src/services/product/src/ProductService.Infrastructure/ProductService.Infrastructure.csproj reference src/buildingblocks/Core/BuildingBlocks.Core.csproj
dotnet add src/services/product/src/ProductService.Infrastructure/ProductService.Infrastructure.csproj reference src/buildingblocks/Shared/BuildingBlocks.Shared.csproj

# Api reference Application + Infrastructure + gRPC
dotnet add src/services/product/src/ProductService.Api/ProductService.Api.csproj reference src/services/product/src/ProductService.Application/ProductService.Application.csproj
dotnet add src/services/product/src/ProductService.Api/ProductService.Api.csproj reference src/services/product/src/ProductService.Infrastructure/ProductService.Infrastructure.csproj
dotnet add src/services/product/src/ProductService.Api/ProductService.Api.csproj reference src/buildingblocks/Core/BuildingBlocks.Core.csproj
dotnet add src/services/product/src/ProductService.Api/ProductService.Api.csproj reference src/buildingblocks/Shared/BuildingBlocks.Shared.csproj
dotnet add src/services/product/src/ProductService.Api/ProductService.Api.csproj reference src/services/product/src/ProductService.gRPC/ProductService.gRPC.csproj

# gRPC reference Domain
dotnet add src/services/product/src/ProductService.gRPC/ProductService.gRPC.csproj reference src/services/product/src/ProductService.Domain/ProductService.Domain.csproj
```

---

## 📝 TASK 5.2: SETUP DOMAIN ENTITIES

### Bước 5.2.1: Thêm reference vào Domain

```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce\src\services\product\src\ProductService.Domain
dotnet add reference ../../../../buildingblocks/Core/BuildingBlocks.Core.csproj
```

### Bước 5.2.2: Tạo Category entity

Tạo file `src/services/product/src/ProductService.Domain/Entities/Category.cs`:

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
        return new Category
        {
            Name = name,
            Description = description,
            SortOrder = sortOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }
}
```

### Bước 5.2.3: Tạo Product entity

Tạo file `src/services/product/src/ProductService.Domain/Entities/Product.cs`:

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

    public static Product Create(
        string name,
        string? description,
        decimal price,
        int stockQuantity,
        Guid categoryId,
        string? sku = null)
    {
        return new Product
        {
            Name = name,
            Description = description,
            Price = price,
            StockQuantity = stockQuantity,
            CategoryId = categoryId,
            SKU = sku ?? GenerateSku(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateStock(int quantity)
    {
        StockQuantity += quantity;
        if (StockQuantity < 0) StockQuantity = 0;
    }

    public void ApplyDiscount(decimal discountPrice)
    {
        DiscountPrice = discountPrice;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string GenerateSku()
    {
        return $"SKU-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }
}
```

### Bước 5.2.4: Tạo IProductRepository

Tạo file `src/services/product/src/ProductService.Domain/Repositories/IProductRepository.cs`:

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

---

## 📝 TASK 5.3: SETUP INFRASTRUCTURE

### Bước 5.3.1: Thêm packages

```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce\src\services\product\src\ProductService.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
```

### Bước 5.3.2: Tạo ProductDbContext

Tạo file `src/services/product/src/ProductService.Infrastructure/Persistence/ProductDbContext.cs`:

```csharp
using ProductService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Infrastructure.Persistence;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.DiscountPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.SKU).HasMaxLength(50);
            entity.HasIndex(e => e.SKU).IsUnique();
            entity.HasIndex(e => e.CategoryId);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Name);
        });
    }
}
```

### Bước 5.3.3: Tạo ProductRepository

Tạo file `src/services/product/src/ProductService.Infrastructure/Persistence/Repositories/ProductRepository.cs`:

```csharp
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Infrastructure.Persistence.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ProductDbContext _context;

    public ProductRepository(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Products.Where(p => p.IsActive).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Products.Where(p => p.CategoryId == categoryId && p.IsActive).ToListAsync(cancellationToken);
    }

    public async Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.FindAsync(new object[] { id }, cancellationToken);
        if (product != null)
        {
            product.Deactivate();
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> IsInStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.FindAsync(new object[] { productId }, cancellationToken);
        return product != null && product.StockQuantity >= quantity;
    }
}
```

---

## 📝 TASK 5.4: IMPLEMENT PRODUCT CRUD

### Bước 5.4.1: Thêm packages vào Application

```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce\src\services\product\src\ProductService.Application
dotnet add package FluentValidation
dotnet add package MediatR
```

### Bước 5.4.2: Tạo DTOs

Tạo file `src/services/product/src/ProductService.Application/DTOs/ProductDtos.cs`:

```csharp
namespace ProductService.Application.DTOs;

public record ProductDto(Guid Id, string Name, string? Description, decimal Price, decimal? DiscountPrice, int StockQuantity, string? ImageUrl, Guid CategoryId, string? SKU);
public record CreateProductRequest(string Name, string? Description, decimal Price, int StockQuantity, Guid CategoryId, string? SKU);
public record UpdateProductRequest(Guid Id, string Name, string? Description, decimal Price, int StockQuantity, Guid CategoryId, decimal? DiscountPrice);
public record CategoryDto(Guid Id, string Name, string? Description, int SortOrder);
```

### Bước 5.4.3: Tạo Commands

Tạo file `src/services/product/src/ProductService.Application/Commands/ProductCommands.cs`:

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

    public CreateProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = Domain.Entities.Product.Create(
            request.Request.Name,
            request.Request.Description,
            request.Request.Price,
            request.Request.StockQuantity,
            request.Request.CategoryId,
            request.Request.SKU
        );

        await _productRepository.AddAsync(product, cancellationToken);

        return new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.DiscountPrice,
            product.StockQuantity,
            product.ImageUrl,
            product.CategoryId,
            product.SKU
        );
    }
}

public record UpdateProductCommand(UpdateProductRequest Request) : IRequest<ProductDto>;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Request.Id).NotEmpty();
        RuleFor(x => x.Request.Name).NotEmpty();
        RuleFor(x => x.Request.Price).GreaterThan(0);
    }
}

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IProductRepository _productRepository;

    public UpdateProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Request.Id, cancellationToken);
        if (product == null)
            throw new Exception("Product not found");

        // Update properties
        product.Name = request.Request.Name;
        product.Description = request.Request.Description;
        product.Price = request.Request.Price;
        product.StockQuantity = request.Request.StockQuantity;
        product.CategoryId = request.Request.CategoryId;

        if (request.Request.DiscountPrice.HasValue)
            product.ApplyDiscount(request.Request.DiscountPrice.Value);

        await _productRepository.UpdateAsync(product, cancellationToken);

        return new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.DiscountPrice,
            product.StockQuantity,
            product.ImageUrl,
            product.CategoryId,
            product.SKU
        );
    }
}

public record DeleteProductCommand(Guid Id) : IRequest<bool>;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IProductRepository _productRepository;

    public DeleteProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        await _productRepository.DeleteAsync(request.Id, cancellationToken);
        return true;
    }
}
```

### Bước 5.4.4: Tạo Queries

Tạo file `src/services/product/src/ProductService.Application/Queries/ProductQueries.cs`:

```csharp
using MediatR;
using ProductService.Application.DTOs;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Queries;

public record GetAllProductsQuery : IRequest<List<ProductDto>>;

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, List<ProductDto>>
{
    private readonly IProductRepository _productRepository;

    public GetAllProductsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<List<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetAllAsync(cancellationToken);

        return products.Select(p => new ProductDto(
            p.Id, p.Name, p.Description, p.Price, p.DiscountPrice,
            p.StockQuantity, p.ImageUrl, p.CategoryId, p.SKU
        )).ToList();
    }
}

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto?>;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IProductRepository _productRepository;

    public GetProductByIdQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product == null) return null;

        return new ProductDto(
            product.Id, product.Name, product.Description, product.Price,
            product.DiscountPrice, product.StockQuantity, product.ImageUrl,
            product.CategoryId, product.SKU
        );
    }
}

public record GetProductsByCategoryQuery(Guid CategoryId) : IRequest<List<ProductDto>>;

public class GetProductsByCategoryQueryHandler : IRequestHandler<GetProductsByCategoryQuery, List<ProductDto>>
{
    private readonly IProductRepository _productRepository;

    public GetProductsByCategoryQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<List<ProductDto>> Handle(GetProductsByCategoryQuery request, CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetByCategoryIdAsync(request.CategoryId, cancellationToken);

        return products.Select(p => new ProductDto(
            p.Id, p.Name, p.Description, p.Price, p.DiscountPrice,
            p.StockQuantity, p.ImageUrl, p.CategoryId, p.SKU
        )).ToList();
    }
}
```

---

## 📝 TASK 5.5: SETUP API LAYER

### Bước 5.5.1: Thêm packages vào Api

```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce\src\services\product\src\ProductService.Api
dotnet add package FluentValidation.AspNetCore
dotnet add package MediatR
dotnet add package Serilog.AspNetCore
```

### Bước 5.5.2: Update Program.cs

Tạo file `src/services/product/src/ProductService.Api/Program.cs`:

```csharp
using ProductService.Application.Commands;
using ProductService.Application.Queries;
using ProductService.Infrastructure.Persistence;
using ProductService.Infrastructure.Persistence.Repositories;
using ProductService.Domain.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly));

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductCommand>();

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Register services
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ProductDbContext>("sqlserver");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
```

### Bước 5.5.3: Tạo appsettings.json

Tạo file `src/services/product/src/ProductService.Api/appsettings.json`:

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
    "DefaultConnection": "Server=localhost,1433;Database=ProductDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True"
  },
  "Urls": "http://0.0.0.0:5002",
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

### Bước 5.5.4: Tạo ProductController

Tạo file `src/services/product/src/ProductService.Api/Controllers/ProductsController.cs`:

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

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetAll()
    {
        return Ok(await _mediator.Send(new GetAllProductsQuery()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<List<ProductDto>>> GetByCategory(Guid categoryId)
    {
        return Ok(await _mediator.Send(new GetProductsByCategoryQuery(categoryId)));
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductRequest request)
    {
        var result = await _mediator.Send(new CreateProductCommand(request));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductDto>> Update(Guid id, [FromBody] UpdateProductRequest request)
    {
        if (id != request.Id) return BadRequest();
        return Ok(await _mediator.Send(new UpdateProductCommand(request)));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> Delete(Guid id)
    {
        return Ok(await _mediator.Send(new DeleteProductCommand(id)));
    }
}
```

---

## 📝 TASK 5.6: SETUP gRPC ENDPOINT

### Bước 5.6.1: Tạo Protobuf file

Tạo file `src/services/product/src/ProductService.gRPC/Protos/product.proto`:

```protobuf
syntax = "proto3";

option csharp_namespace = "ProductService.gRPC";

package ProductService;

service ProductGrpc {
  rpc GetProduct (ProductRequest) returns (ProductResponse);
  rpc CheckStock (StockRequest) returns (StockResponse);
  rpc ReduceStock (StockReductionRequest) returns (StockResponse);
}

message ProductRequest {
  string product_id = 1;
}

message ProductResponse {
  string id = 1;
  string name = 2;
  string description = 3;
  double price = 4;
  double discount_price = 5;
  int32 stock_quantity = 6;
  string sku = 7;
  bool is_valid = 8;
}

message StockRequest {
  string product_id = 1;
  int32 quantity = 2;
}

message StockReductionRequest {
  string product_id = 1;
  int32 quantity = 2;
}

message StockResponse {
  bool success = 1;
  string message = 2;
}
```

### Bước 5.6.2: Tạo gRPC Service

Tạo file `src/services/product/src/ProductService.gRPC/Services/ProductGrpcService.cs`:

```csharp
using Grpc.Core;
using ProductService.gRPC;
using ProductService.Domain.Repositories;

namespace ProductService.gRPC.Services;

public class ProductGrpcService : ProductGrpc.ProductGrpcBase
{
    private readonly IProductRepository _productRepository;

    public ProductGrpcService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public override async Task<ProductResponse> GetProduct(ProductRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ProductId, out var productId))
        {
            return new ProductResponse { IsValid = false };
        }

        var product = await _productRepository.GetByIdAsync(productId);

        if (product == null)
        {
            return new ProductResponse { IsValid = false };
        }

        return new ProductResponse
        {
            Id = product.Id.ToString(),
            Name = product.Name,
            Description = product.Description ?? "",
            Price = (double)product.Price,
            DiscountPrice = product.DiscountPrice.HasValue ? (double)product.DiscountPrice.Value : 0,
            StockQuantity = product.StockQuantity,
            Sku = product.SKU ?? "",
            IsValid = true
        };
    }

    public override async Task<StockResponse> CheckStock(StockRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ProductId, out var productId))
        {
            return new StockResponse { Success = false, Message = "Invalid product ID" };
        }

        var isInStock = await _productRepository.IsInStockAsync(productId, request.Quantity);

        return new StockResponse
        {
            Success = isInStock,
            Message = isInStock ? "Stock available" : "Insufficient stock"
        };
    }

    public override async Task<StockResponse> ReduceStock(StockReductionRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ProductId, out var productId))
        {
            return new StockResponse { Success = false, Message = "Invalid product ID" };
        }

        var product = await _productRepository.GetByIdAsync(productId);

        if (product == null)
        {
            return new StockResponse { Success = false, Message = "Product not found" };
        }

        if (product.StockQuantity < request.Quantity)
        {
            return new StockResponse { Success = false, Message = "Insufficient stock" };
        }

        product.UpdateStock(-request.Quantity);
        await _productRepository.UpdateAsync(product);

        return new StockResponse
        {
            Success = true,
            Message = "Stock reduced successfully"
        };
    }
}
```

### Bước 5.6.3: Update Program.cs để map gRPC

Thêm vào `src/services/product/src/ProductService.Api/Program.cs`:

```csharp
// Sau phần app.MapControllers()
app.MapGrpcService<ProductService.gRPC.Services.ProductGrpcService>();
```

Cần thêm reference và using:

```csharp
using ProductService.gRPC.Services;
```

---

## 📝 TASK 5.7: CREATE MIGRATION VÀ BUILD

### Bước 5.7.1: Create migration

```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce\src\services\product\src\ProductService.Api
dotnet ef migrations add InitialCreate --output-dir ../ProductService.Infrastructure/Persistence/Migrations
```

### Bước 5.7.2: Build solution

```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce
dotnet build
```

---

## ✅ CHECKLIST GIAI ĐOẠN 5

| Task | Status | Ghi chú |
|------|--------|---------|
| 5.1.1 | ⬜ | Tạo Domain project |
| 5.1.2 | ⬜ | Tạo Application project |
| 5.1.3 | ⬜ | Tạo Infrastructure project |
| 5.1.4 | ⬜ | Tạo Api project |
| 5.1.5 | ⬜ | Tạo gRPC project |
| 5.1.6 | ⬜ | Add project references |
| 5.2.1 | ⬜ | Add reference vào Domain |
| 5.2.2 | ⬜ | Tạo Category entity |
| 5.2.3 | ⬜ | Tạo Product entity |
| 5.2.4 | ⬜ | Tạo IProductRepository |
| 5.3.1 | ⬜ | Thêm packages Infrastructure |
| 5.3.2 | ⬜ | Tạo ProductDbContext |
| 5.3.3 | ⬜ | Tạo ProductRepository |
| 5.4.1 | ⬜ | Thêm packages Application |
| 5.4.2 | ⬜ | Tạo DTOs |
| 5.4.3 | ⬜ | Tạo Commands |
| 5.4.4 | ⬜ | Tạo Queries |
| 5.5.1 | ⬜ | Thêm packages Api |
| 5.5.2 | ⬜ | Update Program.cs |
| 5.5.3 | ⬜ | Tạo appsettings.json |
| 5.5.4 | ⬜ | Tạo ProductController |
| 5.6.1 | ⬜ | Tạo Protobuf file |
| 5.6.2 | ⬜ | Tạo gRPC Service |
| 5.6.3 | ⬜ | Map gRPC trong Program.cs |
| 5.7.1 | ⬜ | Create migration |
| 5.7.2 | ⬜ | Build solution |

---

## ❓ KHI HOÀN THÀNH

Khi tất cả tasks hoàn thành, reply:
> **"Done Phase 5"**

Tôi sẽ hướng dẫn tiếp **Giai đoạn 6: OrderService**