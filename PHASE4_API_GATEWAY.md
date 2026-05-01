# 🎯 GIAI ĐOẠN 4: API GATEWAY (YARP)

---

## Bước 4.1: Tạo GatewayService.Api Project

```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce
dotnet new webapi -n GatewayService.Api -o src/services/gateway/src/GatewayService.Api
dotnet sln add src/services/gateway/src/GatewayService.Api/GatewayService.Api.csproj
```

> **Giải thích:**
> - **Web API project**: Entry point của hệ thống
> - **GatewayService.Api**: Nhận tất cả requests từ clients và route đến các services

---

## Bước 4.2: Update .csproj File

Mở và update `src/services/gateway/src/GatewayService.Api/GatewayService.Api.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Yarp.ReverseProxy" Version="2.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
    <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
  </ItemGroup>

</Project>
```

> **Giải thích:**
> - **Yarp.ReverseProxy**: Microsoft reverse proxy cho microservices
> - **JwtBearer**: Validate JWT tokens tại Gateway (thay vì mỗi service)
> - **Serilog**: Logging cho Gateway
> - **Swashbuckle**: Swagger/OpenAPI support

---

## Bước 4.3: Tạo Program.cs

Tạo file `src/services/gateway/src/GatewayService.Api/Program.cs`:

```csharp
using Yarp.ReverseProxy.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)
            )
        };
    });

builder.Services.AddAuthorization();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddHealthChecks();

var app = builder.Build();

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

> **Giải thích:**
> - **AddReverseProxy()**: Enable YARP reverse proxy
> - **LoadFromConfig()**: Load routing config từ appsettings.json
> - **MapReverseProxy()**: Map YARP endpoint
> - Gateway sẽ validate JWT trước khi forward requests
> - **Lưu ý**: Các using statements rất quan trọng - thiếu sẽ gây compile errors

---

## Bước 4.4: Tạo appsettings.json với Routing Config

Tạo file `src/services/gateway/src/GatewayService.Api/appsettings.json`:

```json
{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*",
  "Urls": "http://0.0.0.0:5000",
  "Jwt": {
    "Secret": "ThisIsASecretKeyForJwtTokenGeneration123456",
    "Issuer": "MicroserviceEcommerce",
    "Audience": "MicroserviceEcommerce"
  },
  "ReverseProxy": {
    "Routes": {
      "identity-route": {
        "ClusterId": "identity-cluster",
        "Match": { "Path": "api/auth/{**catch-all}" },
        "Transforms": [{ "PathRemovePrefix": "/api" }]
      },
      "product-route": {
        "ClusterId": "product-cluster",
        "Match": { "Path": "products/{**catch-all}" },
        "Transforms": [{ "PathRemovePrefix": "/products" }]
      },
      "order-route": {
        "ClusterId": "order-cluster",
        "Match": { "Path": "orders/{**catch-all}" },
        "Transforms": [{ "PathRemovePrefix": "/orders" }]
      }
    },
    "Clusters": {
      "identity-cluster": {
        "Destinations": { "identity-service": { "Address": "http://host.docker.internal:5001" } }
      },
      "product-cluster": {
        "Destinations": { "product-service": { "Address": "http://host.docker.internal:5002" } }
      },
      "order-cluster": {
        "Destinations": { "order-service": { "Address": "http://host.docker.internal:5003" } }
      }
    }
  },
  "Serilog": { "MinimumLevel": { "Default": "Information" }, "WriteTo": [{ "Name": "Console" }] }
}
```

> **Giải thích:**
> - **Routes**: Map incoming URLs đến backend services
> - **{**catch-all}**: Wildcard - match tất cả sub-paths
> - **Transforms**: Remove path prefix trước khi forward
> - **Clusters**: Backend service groups
> - **host.docker.internal**: DNS để truy cập host từ container (dùng localhost nếu chạy local)

---

## Bước 4.5: Build Gateway

```bash
dotnet build src/services/gateway/src/GatewayService.Api/GatewayService.Api.csproj
```

> **Giải thích:**
> - Build để kiểm tra code compile không có lỗi

---

## ✅ CHECKLIST GIAI ĐOẠN 4

| Task | Mô tả | Status |
|------|-------|--------|
| 4.1 | Tạo Gateway project | ⬜ |
| 4.2 | Update .csproj | ⬜ |
| 4.3 | Tạo Program.cs | ⬜ |
| 4.4 | Tạo appsettings.json | ⬜ |
| 4.5 | Build | ⬜ |

---

## ❓ KHI HOÀN THÀNH

Reply: **"Done Phase 4"**

Tôi sẽ hướng dẫn tiếp **Giai đoạn 5: Product Service**