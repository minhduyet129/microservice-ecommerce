# 🎯 GIAI ĐOẠN 3: IDENTITY SERVICE
## Thời gian: Day 7-12
## Mục tiêu: Tạo service xác thực và quản lý người dùng

---

## 📝 TASK 3.1: TẠO 4 LAYER PROJECTS

### Bước 3.1.1: Tạo Domain project

```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce

# IdentityService.Domain
dotnet new classlib -n IdentityService.Domain -o src/services/identity/src/IdentityService.Domain
rm src/services/identity/src/IdentityService.Domain/Class1.cs

# Add to solution
dotnet sln add src/services/identity/src/IdentityService.Domain/IdentityService.Domain.csproj
```

### Bước 3.1.2: Tạo Application project

```bash
# IdentityService.Application
dotnet new classlib -n IdentityService.Application -o src/services/identity/src/IdentityService.Application
rm src/services/identity/src/IdentityService.Application/Class1.cs

dotnet sln add src/services/identity/src/IdentityService.Application/IdentityService.Application.csproj
```

### Bước 3.1.3: Tạo Infrastructure project

```bash
# IdentityService.Infrastructure
dotnet new classlib -n IdentityService.Infrastructure -o src/services/identity/src/IdentityService.Infrastructure
rm src/services/identity/src/IdentityService.Infrastructure/Class1.cs

dotnet sln add src/services/identity/src/IdentityService.Infrastructure/IdentityService.Infrastructure.csproj
```

### Bước 3.1.4: Tạo Api project (Web API)

```bash
# IdentityService.Api
dotnet new webapi -n IdentityService.Api -o src/services/identity/src/IdentityService.Api

dotnet sln add src/services/identity/src/IdentityService.Api/IdentityService.Api.csproj
```

### Bước 3.1.5: Add project references

```bash
# Application reference Domain
dotnet add src/services/identity/src/IdentityService.Application/IdentityService.Application.csproj reference src/services/identity/src/IdentityService.Domain/IdentityService.Domain.csproj

# Infrastructure reference Domain + Application
dotnet add src/services/identity/src/IdentityService.Infrastructure/IdentityService.Infrastructure.csproj reference src/services/identity/src/IdentityService.Domain/IdentityService.Domain.csproj
dotnet add src/services/identity/src/IdentityService.Infrastructure/IdentityService.Infrastructure.csproj reference src/services/identity/src/IdentityService.Application/IdentityService.Application.csproj
dotnet add src/services/identity/src/IdentityService.Infrastructure/IdentityService.Infrastructure.csproj reference src/buildingblocks/Core/BuildingBlocks.Core.csproj
dotnet add src/services/identity/src/IdentityService.Infrastructure/IdentityService.Infrastructure.csproj reference src/buildingblocks/Shared/BuildingBlocks.Shared.csproj

# Api reference Application + Infrastructure + BuildingBlocks
dotnet add src/services/identity/src/IdentityService.Api/IdentityService.Api.csproj reference src/services/identity/src/IdentityService.Application/IdentityService.Application.csproj
dotnet add src/services/identity/src/IdentityService.Api/IdentityService.Api.csproj reference src/services/identity/src/IdentityService.Infrastructure/IdentityService.Infrastructure.csproj
dotnet add src/services/identity/src/IdentityService.Api/IdentityService.Api.csproj reference src/buildingblocks/Core/BuildingBlocks.Core.csproj
dotnet add src/services/identity/src/IdentityService.Api/IdentityService.Api.csproj reference src/buildingblocks/Shared/BuildingBlocks.Shared.csproj
```

---

## 📝 TASK 3.2: SETUP DOMAIN ENTITIES

### Bước 3.2.1: Thêm packages vào Domain

```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce\src\services\identity\src\IdentityService.Domain

# Thêm reference tới BuildingBlocks.Core
dotnet add reference ../../../../buildingblocks/Core/BuildingBlocks.Core.csproj
```

### Bước 3.2.2: Tạo User entity

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

### Bước 3.2.3: Tạo UserRole entity

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

### Bước 3.2.4: Tạo UserRepository interface

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

---

## 📝 TASK 3.3: SETUP EF CORE DBCONTEXT

### Bước 3.3.1: Thêm packages vào Infrastructure

```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce\src\services\identity\src\IdentityService.Infrastructure

dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package BCrypt.Net-Next
```

### Bước 3.3.2: Tạo AppDbContext

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
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RoleName).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => new { e.UserId, e.RoleName }).IsUnique();
        });
    }
}
```

### Bước 3.3.3: Tạo UserRepository implementation

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

### Bước 3.3.4: Tạo DatabaseSeeder

Tạo file `src/services/identity/src/IdentityService.Infrastructure/Persistence/Seed/DatabaseSeeder.cs`:

```csharp
using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (!await context.Users.AnyAsync())
        {
            var adminUser = User.Create(
                email: "admin@ecommerce.com",
                passwordHash: BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                fullName: "Administrator",
                phoneNumber: "+1234567890"
            );

            context.Users.Add(adminUser);

            var adminRole = UserRole.Create(adminUser.Id, "Admin");
            var userRole = UserRole.Create(adminUser.Id, "User");

            context.UserRoles.AddRange(adminRole, userRole);

            await context.SaveChangesAsync();
        }
    }
}
```

---

## 📝 TASK 3.4: IMPLEMENT APPLICATION LAYER

### Bước 3.4.1: Thêm packages vào Application

```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce\src\services\identity\src\IdentityService.Application

dotnet add package FluentValidation
dotnet add package MediatR
```

### Bước 3.4.2: Tạo DTOs

Tạo file `src/services/identity/src/IdentityService.Application/DTOs/AuthDtos.cs`:

```csharp
namespace IdentityService.Application.DTOs;

public record RegisterRequest(string Email, string Password, string FullName, string? PhoneNumber);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, string Email, string FullName, List<string> Roles);
```

### Bước 3.4.3: Tạo RegisterCommand

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

### Bước 3.4.4: Tạo LoginQuery

Tạo file `src/services/identity/src/IdentityService.Application/Queries/LoginQuery.cs`:

```csharp
using IdentityService.Application.DTOs;
using IdentityService.Domain.Repositories;
using MediatR;
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

---

## 📝 TASK 3.5: SETUP API LAYER

### Bước 3.5.1: Update Program.cs

Đọc file hiện tại `src/services/identity/src/IdentityService.Api/Program.cs`:

```bash
cat src/services/identity/src/IdentityService.Api/Program.cs
```

Sau đó thay thế bằng:

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
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RegisterCommand).Assembly));

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterCommand>();

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

// Register services
builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
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

### Bước 3.5.2: Tạo appsettings.json

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
    "DefaultConnection": "Server=localhost,1433;Database=IdentityDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True"
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

### Bước 3.5.3: Tạo AuthController

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

### Bước 3.5.4: Tạo Health Check

Tạo file `src/services/identity/src/IdentityService.Api/HealthChecks/DbHealthCheck.cs`:

```csharp
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Data.SqlClient;

namespace IdentityService.Api.HealthChecks;

public class SqlServerHealthCheck : IHealthCheck
{
    private readonly string _connectionString;

    public SqlServerHealthCheck(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            return HealthCheckResult.Healthy("SQL Server is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("SQL Server is unhealthy", ex);
        }
    }
}
```

### Bước 3.5.5: Update Program.cs thêm Health Checks

Thêm vào Program.cs (sau phần builder.Services):

```csharp
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("Identity Service is running"))
    .AddDbContextCheck<AppDbContext>("sqlserver");
```

---

## 📝 TASK 3.6: TẠO MIGRATION VÀ DATABASE

### Bước 3.6.1: Add EF Core tools

```bash
dotnet tool install --global dotnet-ef
```

### Bước 3.6.2: Tạo Migration

```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce\src\services\identity\src\IdentityService.Api

dotnet ef migrations add InitialCreate --output-dir ../IdentityService.Infrastructure/Persistence/Migrations
```

### Bước 3.6.3: Verify project build

```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce
dotnet build src/services/identity/src/IdentityService.Api/IdentityService.Api.csproj
```

---

## ✅ CHECKLIST GIAI ĐOẠN 3

| Task | Status | Ghi chú |
|------|--------|---------|
| 3.1.1 | ⬜ | Tạo Domain project |
| 3.1.2 | ⬜ | Tạo Application project |
| 3.1.3 | ⬜ | Tạo Infrastructure project |
| 3.1.4 | ⬜ | Tạo Api project |
| 3.1.5 | ⬜ | Add project references |
| 3.2.1 | ⬜ | Thêm packages vào Domain |
| 3.2.2 | ⬜ | Tạo User entity |
| 3.2.3 | ⬜ | Tạo UserRole entity |
| 3.2.4 | ⬜ | Tạo IUserRepository |
| 3.3.1 | ⬜ | Thêm packages Infrastructure |
| 3.3.2 | ⬜ | Tạo AppDbContext |
| 3.3.3 | ⬜ | Tạo UserRepository |
| 3.3.4 | ⬜ | Tạo DatabaseSeeder |
| 3.4.1 | ⬜ | Thêm packages Application |
| 3.4.2 | ⬜ | Tạo DTOs |
| 3.4.3 | ⬜ | Tạo RegisterCommand |
| 3.4.4 | ⬜ | Tạo LoginQuery |
| 3.5.1 | ⬜ | Update Program.cs |
| 3.5.2 | ⬜ | Tạo appsettings.json |
| 3.5.3 | ⬜ | Tạo AuthController |
| 3.5.4 | ⬜ | Tạo Health Check |
| 3.5.5 | ⬜ | Add Health Checks |
| 3.6.1 | ⬜ | Install EF tools |
| 3.6.2 | ⬜ | Tạo migration |
| 3.6.3 | ⬜ | Build project |

---

## ❓ KHI HOÀN THÀNH

Khi tất cả tasks hoàn thành, reply:
> **"Done Phase 3"**

Tôi sẽ hướng dẫn tiếp **Giai đoạn 4: API Gateway - YARP**