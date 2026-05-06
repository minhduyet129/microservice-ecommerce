# 🎯 GIAI ĐOẠN 7A: FIX CÁC MỤC CHƯA HOÀN THÀNH (Phần 1)

---

## Mục tiêu Phase này:
1. ✅ Cập nhật Gateway routes (Product, Order)
2. ✅ Fix MassTransit (dùng version 7.1.5 thay 8.x)
3. ✅ Thêm Polly Circuit Breaker vào OrderService

---

## Bước 7A-1: Cập nhật Gateway Routes

### Mục đích:
- Gateway hiện tại có path transform không đúng
- ProductService dùng `/api/products` nhưng Gateway transform sai

### Cách làm:

Mở file `src/services/gateway/src/GatewayService.Api/appsettings.json`:

**Thay thế phần ReverseProxy như sau:**

```json
"ReverseProxy": {
  "Routes": {
    "identity-route": {
      "ClusterId": "identity-cluster",
      "Match": { "Path": "api/auth/{**catch-all}" }
    },
    "product-route": {
      "ClusterId": "product-cluster",
      "Match": { "Path": "api/products/{**catch-all}" }
    },
    "order-route": {
      "ClusterId": "order-cluster",
      "Match": { "Path": "api/orders/{**catch-all}" }
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
}
```

> **Giải thích:**
> - Bỏ Transform vì services đã có prefix `api/` trong route
> - Identity: `/api/auth/*` → `http://5001/api/auth/*`
> - Product: `/api/products/*` → `http://5002/api/products/*`
> - Order: `/api/orders/*` → `http://5003/api/orders/*`

---

## Bước 7A-2: Fix MassTransit Version

### Mục đích:
- MassTransit 8.x không có version stable trên NuGet
- Dùng MassTransit 7.1.5 (stable, phổ biến trong production)

### Cách làm:

**1. Update OrderService.Api.csproj:**

Mở `src/services/order/src/OrderService.Api/OrderService.Api.csproj`:

```xml
<!-- Tìm dòng này và bỏ comment -->
<PackageReference Include="MassTransit.AspNetCore" Version="7.1.5" />
```

**2. Update OrderService.Infrastructure.csproj:**

Mở `src/services/order/src/OrderService.Infrastructure/OrderService.Infrastructure.csproj`:

```xml
<!-- Tìm và thay thế dòng này -->
<PackageReference Include="MassTransit.RabbitMQ" Version="7.1.5" />
```

**3. Update OrderService.Api/Program.cs:**

Mở `src/services/order/src/OrderService.Api/Program.cs`:

**Thêm using:**
```csharp
using MassTransit;
```

**Uncomment phần MassTransit:**
```csharp
// MassTransit commented out do NuGet package issue - UNCOMMENT NOW
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
```

> **Giải thích:**
> - MassTransit 7.1.5 là version stable cuối cùng của dòng 7.x
> - Hoạt động tốt với .NET 8.0
> - RabbitMQ config giữ nguyên

---

## Bước 7A-3: Thêm Polly Circuit Breaker

### Mục đích:
- Khi ProductService không phản hồi, OrderService không bị block vô thời hạn
- Circuit Breaker: sau N lỗi liên tiếp → mở circuit → không gọi nữa
- Sau timeout → thử lại

### Cách làm:

**1. Update OrderService.Infrastructure.csproj:**

Thêm Polly.Extensions.Http:

```xml
<PackageReference Include="Polly" Version="8.0.0" />
<PackageReference Include="Polly.Extensions.Http" Version="3.0.0" />
```

**2. Tạo file retry policy:**

Tạo file `src/services/order/src/OrderService.Infrastructure/Resilience/GrpcPolicies.cs`:

```csharp
using Polly;
using Polly.Retry;
using Grpc.Net.ClientFactory;

namespace OrderService.Infrastructure.Resilience;

public static class GrpcPolicies
{
    public static IHttpClientBuilder AddRetryPolicy(this IHttpClientBuilder builder)
    {
        return builder.AddPolicyHandler(GetRetryPolicy());
    }

    private static AsyncRetryPolicy<System.Net.Http.HttpResponseMessage> GetRetryPolicy()
    {
        return Policy<System.Net.Http.HttpResponseMessage>
            .Handle<Exception>()
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    Console.WriteLine($"Retry {retryAttempt} after {timespan.TotalSeconds}s due to: {outcome.Exception?.Message}");
                });
    }
}
```

**3. Update Program.cs để dùng Polly:**

Trong `src/services/order/src/OrderService.Api/Program.cs`:

```csharp
// Thêm Polly vào gRPC client
builder.Services.AddGrpcClient<ProductGrpc.ProductGrpcClient>(options =>
{
    options.Address = new Uri(builder.Configuration["GrpcSettings:ProductServiceUrl"]!);
})
.AddPolicyHandler(GetRetryPolicy());

// Thêm method mới:
static IAsyncPolicy<System.Net.Http.HttpResponseMessage> GetRetryPolicy()
{
    return Policy<System.Net.Http.HttpResponseMessage>
        .Handle<Exception>()
        .WaitAndRetryAsync(3, retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}
```

> **Giải thích:**
> - Retry 3 lần với exponential backoff (2s, 4s, 8s)
> - Nếu ProductService down, không block vô hạn
> - Có thể mở rộng thành Circuit Breaker sau

---

## Bước 7A-4: Build để kiểm tra

```bash
# Build Gateway
dotnet build src/services/gateway/src/GatewayService.Api

# Build Order Service
dotnet build src/services/order/src/OrderService.Api
```

---

## ✅ CHECKLIST GIAI ĐOẠN 7A

| Task | Mô tả | Status |
|------|-------|--------|
| 7A-1 | Update Gateway routes | ⬜ |
| 7A-2 | Fix MassTransit (7.1.5) | ⬜ |
| 7A-3 | Add Polly retry policy | ⬜ |
| 7A-4 | Build verification | ⬜ |

---

## ❓ KHI HOÀN THÀNH

Reply: **"Done Phase 7A"**

Tôi sẽ hướng dẫn tiếp **Giai đoạn 7B: Unit Tests + Serilog + Update README**