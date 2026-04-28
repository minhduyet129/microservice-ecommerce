# 🎯 GIAI ĐOẠN 4: API GATEWAY - YARP
## Thời gian: Day 13-15
## Mục tiêu: Tạo API Gateway để routing và authentication entry point

---

## 📝 TASK 4.1: TẠO GATEWAY SERVICE PROJECT

### Bước 4.1.1: Tạo Gateway project

```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce

# Tạo project
dotnet new webapi -n GatewayService.Api -o src/services/gateway/src/GatewayService.Api

# Add vào solution
dotnet sln add src/services/gateway/src/GatewayService.Api/GatewayService.Api.csproj
```

### Bước 4.1.2: Thêm YARP packages

```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce\src\services\gateway\src\GatewayService.Api

dotnet add package Yarp.ReverseProxy
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Serilog.AspNetCore
```

---

## 📝 TASK 4.2: CONFIG YARP ROUTING

### Bước 4.2.1: Update Program.cs

Đọc và thay thế `src/services/gateway/src/GatewayService.Api/Program.cs`:

```csharp
using GatewayService.Api;
using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add JWT Authentication
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
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
```

### Bước 4.2.2: Tạo appsettings.json

Tạo file `src/services/gateway/src/GatewayService.Api/appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Jwt": {
    "Secret": "ThisIsASecretKeyForJwtTokenGeneration123456",
    "Issuer": "MicroserviceEcommerce",
    "Audience": "MicroserviceEcommerce"
  },
  "ReverseProxy": {
    "Routes": {
      "identity-route": {
        "ClusterId": "identity-cluster",
        "Match": {
          "Path": "api/auth/{**catch-all}"
        },
        "Transforms": [
          {
            "PathRemovePrefix": "/api"
          }
        ]
      },
      "product-route": {
        "ClusterId": "product-cluster",
        "Match": {
          "Path": "products/{**catch-all}"
        },
        "Transforms": [
          {
            "PathRemovePrefix": "/products"
          }
        ]
      },
      "order-route": {
        "ClusterId": "order-cluster",
        "Match": {
          "Path": "orders/{**catch-all}"
        },
        "Transforms": [
          {
            "PathRemovePrefix": "/orders"
          }
        ]
      }
    },
    "Clusters": {
      "identity-cluster": {
        "Destinations": {
          "identity-service": {
            "Address": "http://host.docker.internal:5001"
          }
        }
      },
      "product-cluster": {
        "Destinations": {
          "product-service": {
            "Address": "http://host.docker.internal:5002"
          }
        }
      },
      "order-cluster": {
        "Destinations": {
          "order-service": {
            "Address": "http://host.docker.internal:5003"
          }
        }
      }
    }
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

**Lưu ý:** `host.docker.internal` là DNS name để truy cập host machine từ container. Nếu chạy local không dùng Docker, thay bằng `localhost`.

---

## 📝 TASK 4.3: UPDATE IDENTITY SERVICE CONFIGURE

### Bước 4.3.1: Cập nhật IdentityService.Api Program.cs để chạy port 5001

```bash
# Mở file src/services/identity/src/IdentityService.Api/Properties/launchSettings.json
# Hoặc update appsettings.json
```

Thêm vào `src/services/identity/src/IdentityService.Api/appsettings.json`:

```json
{
  "Urls": "http://0.0.0.0:5001"
}
```

---

## 📝 TASK 4.4: TEST ROUTING

### Bước 4.4.1: Update docker-compose

Cập nhật `infrastructure/docker-compose.yml` thêm các services:

```yaml
version: '3.8'

services:
  # SQL Server
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: ecommerce_sqlserver
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong!Passw0rd
      - MSSQL_PID=Developer
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
    networks:
      - ecommerce-network

  # RabbitMQ
  rabbitmq:
    image: rabbitmq:3-management
    container_name: ecommerce_rabbitmq
    environment:
      - RABBITMQ_DEFAULT_USER=guest
      - RABBITMQ_DEFAULT_PASS=guest
    ports:
      - "5672:5672"
      - "15672:15672"
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
    networks:
      - ecommerce-network

  # Identity Service
  identity-service:
    build:
      context: ../..
      dockerfile: src/services/identity/src/IdentityService.Api/Dockerfile
    container_name: ecommerce_identity
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=sqlserver,1433;Database=IdentityDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True
      - Jwt__Secret=ThisIsASecretKeyForJwtTokenGeneration123456
      - Jwt__Issuer=MicroserviceEcommerce
      - Jwt__Audience=MicroserviceEcommerce
    ports:
      - "5001:80"
    depends_on:
      sqlserver:
        condition: service_healthy
    networks:
      - ecommerce-network
    extra_hosts:
      - "host.docker.internal:host-gateway"

  # Gateway Service
  gateway-service:
    build:
      context: ../..
      dockerfile: src/services/gateway/src/GatewayService.Api/Dockerfile
    container_name: ecommerce_gateway
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    ports:
      - "5000:80"
    depends_on:
      - identity-service
    networks:
      - ecommerce-network

volumes:
  sqlserver_data:
  rabbitmq_data:

networks:
  ecommerce-network:
    driver: bridge
```

---

## ✅ CHECKLIST GIAI ĐOẠN 4

| Task | Status | Ghi chú |
|------|--------|---------|
| 4.1.1 | ⬜ | Tạo Gateway project |
| 4.1.2 | ⬜ | Add YARP packages |
| 4.2.1 | ⬜ | Update Program.cs |
| 4.2.2 | ⬜ | Tạo appsettings.json với routes |
| 4.3.1 | ⬜ | Config Identity port 5001 |
| 4.4.1 | ⬜ | Update docker-compose |

---

## ❓ KHI HOÀN THÀNH

Khi tất cả tasks hoàn thành, reply:
> **"Done Phase 4"**

Tôi sẽ hướng dẫn tiếp **Giai đoạn 5: ProductService**