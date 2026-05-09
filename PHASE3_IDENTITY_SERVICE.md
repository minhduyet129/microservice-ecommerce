# 🎯 GIAI ĐOẠN 3: IDENTITY SERVICE (AUTHENTICATION & AUTHORIZATION)

---

## Bước 3.1: Tạo IdentityService.Domain Project

```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce
dotnet new classlib -n IdentityService.Domain -o src/services/identity/src/IdentityService.Domain
rm src/services/identity/src/IdentityService.Domain/Class1.cs
dotnet sln add src/services/identity/src/IdentityService.Domain/IdentityService.Domain.csproj
```

> **Giải thích:**
> - **Domain layer**: Chứa business logic và rules, không phụ thuộc gì cả
> - **IdentityService.Domain**: Chứa User entity, UserRole entity, interfaces
> - Mỗi service đều có Domain project riêng

---

## Bước 3.2: Tạo IdentityService.Application Project

```bash
dotnet new classlib -n IdentityService.Application -o src/services/identity/src/IdentityService.Application
rm src/services/identity/src/IdentityService.Application/Class1.cs
dotnet sln add src/services/identity/src/IdentityService.Application/IdentityService.Application.csproj
```

> **Giải thích:**
> - **Application layer**: Chứa use cases (Commands và Queries theo CQRS pattern)
> - **IdentityService.Application**: Chứa RegisterCommand, LoginQuery, validators

---

## Bước 3.3: Tạo IdentityService.Infrastructure Project

```bash
dotnet new classlib -n IdentityService.Infrastructure -o src/services/identity/src/IdentityService.Infrastructure
rm src/services/identity/src/IdentityService.Infrastructure/Class1.cs
dotnet sln add src/services/identity/src/IdentityService.Infrastructure/IdentityService.Infrastructure.csproj
```

> **Giải thích:**
> - **Infrastructure layer**: Chứa implementations (EF Core, repositories)
> - **IdentityService.Infrastructure**: Chứa AppDbContext, UserRepository implementation

---

## Bước 3.4: Tạo IdentityService.Api Project

```bash
dotnet new webapi -n IdentityService.Api -o src/services/identity/src/IdentityService.Api
dotnet sln add src/services/identity/src/IdentityService.Api/IdentityService.Api.csproj
```

> **Giải thích:**
> - **Web API project**: Entry point của service, nhận HTTP requests
> - **IdentityService.Api**: Controllers, Middleware, Swagger, Health checks

---

## Bước 3.5: Add Project References

```bash
dotnet add src/services/identity/src/IdentityService.Application/IdentityService.Application.csproj reference src/services/identity/src/IdentityService.Domain/IdentityService.Domain.csproj

dotnet add src/services/identity/src/IdentityService.Application/IdentityService.Application.csproj reference src/buildingblocks/Shared/BuildingBlocks.Shared.csproj

dotnet add src/services/identity/src/IdentityService.Infrastructure/IdentityService.Infrastructure.csproj reference src/services/identity/src/IdentityService.Domain/IdentityService.Domain.csproj
dotnet add src/services/identity/src/IdentityService.Infrastructure/IdentityService.Infrastructure.csproj reference src/services/identity/src/IdentityService.Application/IdentityService.Application.csproj
dotnet add src/services/identity/src/IdentityService.Infrastructure/IdentityService.Infrastructure.csproj reference src/buildingblocks/Core/BuildingBlocks.Core.csproj
dotnet add src/services/identity/src/IdentityService.Infrastructure/IdentityService.Infrastructure.csproj reference src/buildingblocks/Shared/BuildingBlocks.Shared.csproj

dotnet add src/services/identity/src/IdentityService.Api/IdentityService.Api.csproj reference src/services/identity/src/IdentityService.Application/IdentityService.Application.csproj
dotnet add src/services/identity/src/IdentityService.Api/IdentityService.Api.csproj reference src/services/identity/src/IdentityService.Infrastructure/IdentityService.Infrastructure.csproj
dotnet add src/services/identity/src/IdentityService.Api/IdentityService.Api.csproj reference src/buildingblocks/Core/BuildingBlocks.Core.csproj
dotnet add src/services/identity/src/IdentityService.Api/IdentityService.Api.csproj reference src/buildingblocks/Shared/BuildingBlocks.Shared.csproj
```

> **Giải thích:**
> - **Clean Architecture rule**: Mỗi layer chỉ reference layer bên dưới
> - Application → Domain (biết business logic)
> - Infrastructure → Domain + Application (biết implementation)
> - Api → Application + Infrastructure (nhận requests và gọi handlers)

---

## Bước 3.6: Update .csproj Files với Packages và Target Framework

Mở và update `IdentityService.Domain.csproj`:

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

Mở và update `IdentityService.Application.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\IdentityService.Domain\IdentityService.Domain.csproj" />
    <ProjectReference Include="..\..\..\..\buildingblocks\Shared\BuildingBlocks.Shared.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="FluentValidation" Version="11.9.0" />
    <PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.9.0" />
    <PackageReference Include="MediatR" Version="12.2.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" Version="8.0.0" />
    <PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.3.1" />
    <PackageReference Include="BCrypt.Net-Next" Version="4.0.0" />
  </ItemGroup>

</Project>
```

Mở và update `IdentityService.Infrastructure.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\IdentityService.Domain\IdentityService.Domain.csproj" />
    <ProjectReference Include="..\IdentityService.Application\IdentityService.Application.csproj" />
    <ProjectReference Include="..\..\..\..\buildingblocks\Core\BuildingBlocks.Core.csproj" />
    <ProjectReference Include="..\..\..\..\buildingblocks\Shared\BuildingBlocks.Shared.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="BCrypt.Net-Next" Version="4.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
  </ItemGroup>

</Project>
```

Mở và update `IdentityService.Api.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
    <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
    <PackageReference Include="AspNetCore.HealthChecks.NpgSql" Version="8.0.0" />
    <PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
    <PackageReference Include="Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore" Version="8.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\IdentityService.Application\IdentityService.Application.csproj" />
    <ProjectReference Include="..\IdentityService.Infrastructure\IdentityService.Infrastructure.csproj" />
    <ProjectReference Include="..\..\..\..\buildingblocks\Core\BuildingBlocks.Core.csproj" />
    <ProjectReference Include="..\..\..\..\buildingblocks\Shared\BuildingBlocks.Shared.csproj" />
  </ItemGroup>

</Project>
```

> **Giải thích:**
> - **TargetFramework net8.0**: .NET 8.0 LTS (Long Term Support)
> - **FluentValidation**: Validate input data (email format, password length)
> - **MediatR**: CQRS pattern - tách command/query khỏi controller
> - **BCrypt.Net-Next**: Hash password an toàn (không lưu plaintext)
> - **JwtBearer**: Validate và generate JWT tokens
> - **Npgsql**: PostgreSQL provider cho Entity Framework
> - **Swashbuckle**: Tự động tạo Swagger documentation
> - **Serilog**: Structured logging thay cho Console.WriteLine
> - **HealthChecks**: Endpoint để load balancer kiểm tra service health

---

## Bước 3.7: Tạo User Entity (Domain Layer)

Tạo file `src/services/identity/src/IdentityService.Domain/Entities/User.cs`:

```csharp
using BuildingBlocks.Core.Abstractions;

namespace IdentityService.Domain.Entities;

public class User : Entity<Guid>
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    public User()
    {
        Id = Guid.NewGuid();
    }

    public static User Create(string email, string passwordHash, string fullName, string? phoneNumber = null)
    {
        return new User
        {
            Email = email,
            PasswordHash = passwordHash,
            FullName = fullName,
            PhoneNumber = phoneNumber,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
```

> **Giải thích:**
> - **Entity<Guid>**: Base class từ BuildingBlocks.Core, có Id là Guid
> - **Factory method Create()**: Tạo user mới với defaults
> - **Business methods**: UpdateLastLogin, Deactivate - encapsulate logic trong entity
> - **PasswordHash**: Lưu hashed password, KHÔNG BAO GIỜ lưu plaintext

---

## Bước 3.8: Tạo UserRole Entity (Domain Layer)

Tạo file `src/services/identity/src/IdentityService.Domain/Entities/UserRole.cs`:

```csharp
using BuildingBlocks.Core.Abstractions;

namespace IdentityService.Domain.Entities;

public class UserRole : Entity<Guid>
{
    public Guid UserId { get; set; }
    public string RoleName { get; set; } = string.Empty;

    public UserRole()
    {
        Id = Guid.NewGuid();
    }

    public static UserRole Create(Guid userId, string roleName)
    {
        return new UserRole
        {
            UserId = userId,
            RoleName = roleName,
            CreatedAt = DateTime.UtcNow
        };
    }
}
```

> **Giải thích:**
> - **UserRole**: Lưu roles của user (Admin, User, Manager, etc.)
> - 1 user có thể có nhiều roles

---

## Bước 3.9: Tại tạo IUserRepository Interface (Domain Layer)

Tạo file `src/services/identity/src/IdentityService.Domain/Repositories/IUserRepository.cs`:

```csharp
using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
}
```

> **Giải thích:**
> - **Interface trong Domain**: Định nghĩa contract, không biết implementation
> - Implementation sẽ ở Infrastructure layer
> - **Repository pattern**: Tách data access khỏi business logic

---

## Bước 3.10: Tạo AppDbContext (Infrastructure Layer)

Tạo file `src/services/identity/src/IdentityService.Infrastructure/Persistence/AppDbContext.cs`:

```csharp
using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("user_roles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RoleName).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => new { e.UserId, e.RoleName }).IsUnique();
        });
    }
}
```

> **Giải thích:**
> - **DbContext**: Cửa ngõ để truy cập database
> - **DbSet<User>**: Represents bảng users
> - **ToTable("users")**: PostgreSQL convention dùng snake_case
> - **HasIndex().IsUnique()**: Email không được trùng

---

## Bước 3.11: Tạo UserRepository Implementation (Infrastructure Layer)

Tạo file `src/services/identity/src/IdentityService.Infrastructure/Persistence/Repositories/UserRepository.cs`:

```csharp
using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);
    }
}
```

> **Giải thích:**
> - **Implement IUserRepository**: Cụ thể hóa interface
> - **FirstOrDefaultAsync**: Lấy first record hoặc null
> - **AnyAsync**: Kiểm tra có tồn tại không

---

## Bước 3.12: Tạo DTOs (Application Layer)

Tạo file `src/services/identity/src/IdentityService.Application/DTOs/AuthDtos.cs`:

```csharp
namespace IdentityService.Application.DTOs;

public record RegisterRequest(string Email, string Password, string FullName, string? PhoneNumber);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, string Email, string FullName, List<string> Roles);
```

> **Giải thích:**
> - **DTO**: Data Transfer Object - truyền data giữa layers
> - **record**: C# immutable type, tự generate Equals()
> - **RegisterRequest**: Input để đăng ký
> - **LoginRequest**: Input để đăng nhập  
> - **AuthResponse**: Output - chứa JWT token

---

## Bước 3.13: Tạo RegisterCommand (Application Layer)

Tạo file `src/services/identity/src/IdentityService.Application/Commands/RegisterCommand.cs`:

```csharp
using FluentValidation;
using IdentityService.Application.DTOs;
using IdentityService.Domain.Repositories;
using MediatR;
using BCrypt.Net;

namespace IdentityService.Application.Commands;

public record RegisterCommand(RegisterRequest Request) : IRequest<AuthResponse>;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Request.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Request.Password).MinimumLength(6);
        RuleFor(x => x.Request.FullName).NotEmpty();
    }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;

    public RegisterCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _userRepository.ExistsByEmailAsync(request.Request.Email, cancellationToken))
        {
            throw new Exception("Email already exists");
        }

        var user = Domain.Entities.User.Create(
            request.Request.Email,
            BCrypt.Net.BCrypt.HashPassword(request.Request.Password),
            request.Request.FullName,
            request.Request.PhoneNumber
        );

        await _userRepository.AddAsync(user, cancellationToken);

        return new AuthResponse(string.Empty, user.Email, user.FullName, new List<string> { "User" });
    }
}
```

> **Giải thích:**
> - **Command Pattern**: Represent một action (register user)
> - **IRequest<AuthResponse>**: Command trả về AuthResponse
> - **AbstractValidator**: FluentValidation rule
> - **BCrypt.HashPassword()**: Hash password trước khi lưu (SECURITY!)
> - **MediatR**: Dispatch command đến handler tương ứng

---

## Bước 3.14: Tạo LoginQuery (Application Layer)

Tạo file `src/services/identity/src/IdentityService.Application/Queries/LoginQuery.cs`:

```csharp
using IdentityService.Application.DTOs;
using IdentityService.Domain.Repositories;
using MediatR;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityService.Application.Queries;

public record LoginQuery(LoginRequest Request) : IRequest<AuthResponse>;

public class LoginQueryValidator : AbstractValidator<LoginQuery>
{
    public LoginQueryValidator()
    {
        RuleFor(x => x.Request.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Request.Password).NotEmpty();
    }
}

public class LoginQueryHandler : IRequestHandler<LoginQuery, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public LoginQueryHandler(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<AuthResponse> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Request.Email, cancellationToken);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Request.Password, user.PasswordHash))
        {
            throw new Exception("Invalid credentials");
        }

        if (!user.IsActive)
        {
            throw new Exception("User account is deactivated");
        }

        user.UpdateLastLogin();

        var token = GenerateJwtToken(user);

        return new AuthResponse(token, user.Email, user.FullName, new List<string> { "User" });
    }

    private string GenerateJwtToken(Domain.Entities.User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, "User")
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

> **Giải thích:**
> - **Query Pattern**: Represent một câu hỏi (get user by email)
> - **BCrypt.Verify()**: So sánh password input với hash trong database
> - **JwtSecurityToken**: Tạo JWT token chứa user info
> - **Claims**: Thông tin được encode trong token (userId, email, role)
> - **Token expiry**: 24 hours

---

## Bước 3.15: Tạo appsettings.json (Api Layer)

Tạo file `src/services/identity/src/IdentityService.Api/appsettings.json`:

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
    "DefaultConnection": "Host=localhost;Port=5432;Database=postgres;Username=sa;Password=YourStrong!Passw0rd"
  },
  "Jwt": {
    "Secret": "ThisIsASecretKeyForJwtTokenGeneration123456",
    "Issuer": "MicroserviceEcommerce",
    "Audience": "MicroserviceEcommerce",
    "ExpirationInHours": 24
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

> **Giải thích:**
> - **ConnectionStrings**: PostgreSQL connection string (Host=localhost, Port=5432)
> - **Jwt**: Settings để generate và validate tokens
> - **Secret**: Key để sign tokens - trong production phải store trong secret manager
> - **Serilog**: Logging configuration

---

## Bước 3.16: Tạo Program.cs (Api Layer)

Tạo file `src/services/identity/src/IdentityService.Api/Program.cs`:

```csharp
using IdentityService.Application.Commands;
using IdentityService.Application.Queries;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Infrastructure.Persistence.Repositories;
using IdentityService.Domain.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RegisterCommand).Assembly));

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterCommand>();

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

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("Identity Service is running"))
    .AddDbContextCheck<AppDbContext>("postgresql");

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

app.Run();
```

> **Giải thích:**
> - **AddDbContext**: Đăng ký EF Core DbContext với DI container
> - **UseNpgsql**: Dùng PostgreSQL provider thay vì SQL Server
> - **AddMediatR**: Tìm và register tất cả handlers trong assembly
> - **AddFluentValidationAutoValidation**: Tự động validate input
> - **AddAuthentication("Bearer")**: Enable JWT authentication
> - **UseSerilog**: Thay thế default logging bằng Serilog
> - **AddHealthChecks**: Endpoint /health cho load balancer

---

## Bước 3.17: Tạo AuthController (Api Layer)

Tạo file `src/services/identity/src/IdentityService.Api/Controllers/AuthController.cs`:

```csharp
using IdentityService.Application.Commands;
using IdentityService.Application.DTOs;
using IdentityService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        var result = await _mediator.Send(new RegisterCommand(request));
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var result = await _mediator.Send(new LoginQuery(request));
        return Ok(result);
    }
}
```

> **Giải thích:**
> - **ApiController**: Tự động validate model, return 400 nếu invalid
> - **Route("api/[controller]")**: URL = /api/auth
> - **[HttpPost]**: Handle POST requests
> - **MediatR.Send()**: Gửi command/query đến handler tương ứng

---

## Bước 3.18: Tạo Database và Chạy Migration

```bash
# Tạo database trong PostgreSQL
docker exec ecommerce_postgres psql -U sa -d postgres -c "CREATE DATABASE identitydb;"

# Build
dotnet build src/services/identity/src/IdentityService.Api/IdentityService.Api.csproj
```

> **Giải thích:**
> - Tạo database riêng cho Identity service
> - Mỗi service có database riêng (Database per Service pattern)

> **⚠️ QUAN TRỌNG: DbContext nằm trong Infrastructure, không phải Api**

```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce

# Tạo migration (--project = Infrastructure chứa DbContext, --startup-project = Api chứa connection string)
dotnet ef migrations add InitialCreate --project src/services/identity/src/IdentityService.Infrastructure --startup-project src/services/identity/src/IdentityService.Api --output-dir Persistence/Migrations

# Apply migration
dotnet ef database update --project src/services/identity/src/IdentityService.Infrastructure --startup-project src/services/identity/src/IdentityService.Api

# Verify tables đã tạo
docker exec ecommerce_postgres psql -U sa -d identitydb -c "\dt"
```

> **Giải thích:**
> - DbContext nằm trong `IdentityService.Infrastructure/Persistence/AppDbContext.cs`
> - Migrations output: `IdentityService.Infrastructure/Persistence/Migrations/`
> - `--project`: Project chứa DbContext (Infrastructure)
> - `--startup-project`: Project chứa appsettings.json với connection string (Api)

---

## Bước 3.19: Build và Test

```bash
dotnet build src/services/identity/src/IdentityService.Api/IdentityService.Api.csproj
```

> **Giải thích:**
> - Build để kiểm tra code compile không có lỗi

```bash
# Chạy service
cd src/services/identity/src/IdentityService.Api
dotnet run
```

> **Giải thích:**
> - Service sẽ chạy ở port 5001 (hoặc random available port)

---

## ✅ CHECKLIST GIAI ĐOẠN 3

| Task | Mô tả | Status |
|------|-------|--------|
| 3.1 | Tạo Domain project | ⬜ |
| 3.2 | Tạo Application project | ⬜ |
| 3.3 | Tạo Infrastructure project | ⬜ |
| 3.4 | Tạo Api project | ⬜ |
| 3.5 | Add project references | ⬜ |
| 3.6 | Update .csproj files | ⬜ |
| 3.7 | Tạo User entity | ⬜ |
| 3.8 | Tạo UserRole entity | ⬜ |
| 3.9 | Tạo IUserRepository | ⬜ |
| 3.10 | Tạo AppDbContext | ⬜ |
| 3.11 | Tạo UserRepository | ⬜ |
| 3.12 | Tạo DTOs | ⬜ |
| 3.13 | Tạo RegisterCommand | ⬜ |
| 3.14 | Tạo LoginQuery | ⬜ |
| 3.15 | Tạo appsettings.json | ⬜ |
| 3.16 | Tạo Program.cs | ⬜ |
| 3.17 | Tạo AuthController | ⬜ |
| 3.18 | Tạo database & migration | ⬜ |
| 3.19 | Build & Test | ⬜ |

---

## ❓ KHI HOÀN THÀNH

Khi tất cả tasks hoàn thành, reply:
> **"Done Phase 3"**

Tôi sẽ hướng dẫn tiếp **Giai đoạn 4: API Gateway - YARP**