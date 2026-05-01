# 🎯 GIAI ĐOẠN 7: DOCKER + CI/CD

---

## Bước 7.1: Tạo Dockerfiles cho từng Service

**IdentityService.Api/Dockerfile:**

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/services/identity/src/IdentityService.Api/IdentityService.Api.csproj", "src/services/identity/src/IdentityService.Api/"]
COPY ["src/services/identity/src/IdentityService.Application/IdentityService.Application.csproj", "src/services/identity/src/IdentityService.Application/"]
COPY ["src/services/identity/src/IdentityService.Domain/IdentityService.Domain.csproj", "src/services/identity/src/IdentityService.Domain/"]
COPY ["src/services/identity/src/IdentityService.Infrastructure/IdentityService.Infrastructure.csproj", "src/services/identity/src/IdentityService.Infrastructure/"]
COPY ["src/buildingblocks/Core/BuildingBlocks.Core.csproj", "src/buildingblocks/Core/"]
COPY ["src/buildingblocks/Shared/BuildingBlocks.Shared.csproj", "src/buildingblocks/Shared/"]

RUN dotnet restore src/services/identity/src/IdentityService.Api/IdentityService.Api.csproj
COPY . .
WORKDIR /src/src/services/identity/src/IdentityService.Api
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IdentityService.Api.dll"]
```

> **Giải thích:**
> - **Multi-stage build**: Build trong SDK image, run trong ASP.NET Core runtime image
> - Giảm size của final image đáng kể
> - Build/release separation cho security

**ProductService.Api/Dockerfile:**

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/services/product/src/ProductService.Api/ProductService.Api.csproj", "src/services/product/src/ProductService.Api/"]
COPY ["src/services/product/src/ProductService.Application/ProductService.Application.csproj", "src/services/product/src/ProductService.Application/"]
COPY ["src/services/product/src/ProductService.Domain/ProductService.Domain.csproj", "src/services/product/src/ProductService.Domain/"]
COPY ["src/services/product/src/ProductService.Infrastructure/ProductService.Infrastructure.csproj", "src/services/product/src/ProductService.Infrastructure/"]
COPY ["src/services/product/src/ProductService.gRPC/ProductService.gRPC.csproj", "src/services/product/src/ProductService.gRPC/"]
COPY ["src/buildingblocks/Core/BuildingBlocks.Core.csproj", "src/buildingblocks/Core/"]
COPY ["src/buildingblocks/Shared/BuildingBlocks.Shared.csproj", "src/buildingblocks/Shared/"]

RUN dotnet restore src/services/product/src/ProductService.Api/ProductService.Api.csproj
COPY . .
WORKDIR /src/src/services/product/src/ProductService.Api
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProductService.Api.dll"]
```

> **Giải thích:**
> - Tương tự Identity nhưng với Product service
> - Cần include gRPC project

**OrderService.Api/Dockerfile:**

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/services/order/src/OrderService.Api/OrderService.Api.csproj", "src/services/order/src/OrderService.Api/"]
COPY ["src/services/order/src/OrderService.Application/OrderService.Application.csproj", "src/services/order/src/OrderService.Application/"]
COPY ["src/services/order/src/OrderService.Domain/OrderService.Domain.csproj", "src/services/order/src/OrderService.Domain/"]
COPY ["src/services/order/src/OrderService.Infrastructure/OrderService.Infrastructure.csproj", "src/services/order/src/OrderService.Infrastructure/"]
COPY ["src/services/product/src/ProductService.gRPC/ProductService.gRPC.csproj", "src/services/product/src/ProductService.gRPC/"]
COPY ["src/buildingblocks/Core/BuildingBlocks.Core.csproj", "src/buildingblocks/Core/"]
COPY ["src/buildingblocks/Shared/BuildingBlocks.Shared.csproj", "src/buildingblocks/Shared/"]

RUN dotnet restore src/services/order/src/OrderService.Api/OrderService.Api.csproj
COPY . .
WORKDIR /src/src/services/order/src/OrderService.Api
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderService.Api.dll"]
```

**GatewayService.Api/Dockerfile:**

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/services/gateway/src/GatewayService.Api/GatewayService.Api.csproj", "src/services/gateway/src/GatewayService.Api/"]
COPY ["src/buildingblocks/Core/BuildingBlocks.Core.csproj", "src/buildingblocks/Core/"]
COPY ["src/buildingblocks/Shared/BuildingBlocks.Shared.csproj", "src/buildingblocks/Shared/"]

RUN dotnet restore src/services/gateway/src/GatewayService.Api/GatewayService.Api.csproj
COPY . .
WORKDIR /src/src/services/gateway/src/GatewayService.Api
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GatewayService.Api.dll"]
```

> **Giải thích:**
> - Gateway nhẹ nhất vì ít dependencies

---

## Bước 7.2: Cập nhật docker-compose.yml

Mở và cập nhật `infrastructure/docker-compose.yml`:

```yaml
services:
  # PostgreSQL
  postgres:
    image: postgres:16-alpine
    container_name: ecommerce_postgres
    environment:
      - POSTGRES_USER=sa
      - POSTGRES_PASSWORD=YourStrong!Passw0rd
      - POSTGRES_DB=postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

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

  # Identity Service
  identity-service:
    build:
      context: ../..
      dockerfile: src/services/identity/src/IdentityService.Api/Dockerfile
    container_name: ecommerce_identity
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=postgres,5432;Database=identitydb;User Id=sa;Password=YourStrong!Passw0rd
      - Jwt__Secret=ThisIsASecretKeyForJwtTokenGeneration123456
      - Jwt__Issuer=MicroserviceEcommerce
      - Jwt__Audience=MicroserviceEcommerce
    ports:
      - "5001:80"
    depends_on:
      postgres:
        condition: service_healthy

  # Product Service
  product-service:
    build:
      context: ../..
      dockerfile: src/services/product/src/ProductService.Api/Dockerfile
    container_name: ecommerce_product
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=postgres,5432;Database=productdb;User Id=sa;Password=YourStrong!Passw0rd
    ports:
      - "5002:80"
    depends_on:
      postgres:
        condition: service_healthy

  # Order Service
  order-service:
    build:
      context: ../..
      dockerfile: src/services/order/src/OrderService.Api/Dockerfile
    container_name: ecommerce_order
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=postgres,5432;Database=orderdb;User Id=sa;Password=YourStrong!Passw0rd
      - Jwt__Secret=ThisIsASecretKeyForJwtTokenGeneration123456
      - Jwt__Issuer=MicroserviceEcommerce
      - Jwt__Audience=MicroserviceEcommerce
      - GrpcSettings__ProductServiceUrl=http://product-service:80
      - RabbitMQ__Host=rabbitmq
      - RabbitMQ__Username=guest
      - RabbitMQ__Password=guest
    ports:
      - "5003:80"
    depends_on:
      postgres:
        condition: service_healthy
      rabbitmq:
        condition: service_started

  # Gateway Service
  gateway-service:
    build:
      context: ../..
      dockerfile: src/services/gateway/src/GatewayService.Api/Dockerfile
    container_name: ecommerce_gateway
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - Jwt__Secret=ThisIsASecretKeyForJwtTokenGeneration123456
      - Jwt__Issuer=MicroserviceEcommerce
      - Jwt__Audience=MicroserviceEcommerce
    ports:
      - "5000:80"
    depends_on:
      - identity-service
      - product-service
      - order-service

volumes:
  postgres_data:
  rabbitmq_data:
```

> **Giải thích:**
> - **build context**: Build từ root của project
> - **depends_on**: Services khởi động theo thứ tự
> - **Service names**: Dùng làm DNS trong Docker network (postgres, rabbitmq, etc.)
> - **Environment variables**: Override appsettings.json values

---

## Bước 7.3: Tạo GitHub Actions Workflow

Tạo thư mục `.github/workflows` và file `ci.yml`:

```bash
mkdir -p .github/workflows
```

**.github/workflows/ci.yml:**

```yaml
name: CI Build and Test

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main, develop]

env:
  DOTNET_VERSION: '8.0.x'

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4

    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}

    - name: Restore dependencies
      run: dotnet restore

    - name: Build
      run: dotnet build --no-restore

    - name: Run tests
      run: dotnet test --no-build --verbosity normal
```

**.github/workflows/docker.yml:**

```yaml
name: Docker Build and Push

on:
  push:
    branches: [main]
    tags:
      - 'v*'

env:
  REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository }}

jobs:
  build-and-push:
    runs-on: ubuntu-latest
    permissions:
      contents: read
      packages: write
    
    steps:
    - name: Checkout
      uses: actions/checkout@v4

    - name: Set up Docker Buildx
      uses: docker/setup-buildx-action@v3

    - name: Login to Container Registry
      uses: docker/login-action@v3
      with:
        registry: ${{ env.REGISTRY }}
        username: ${{ github.actor }}
        password: ${{ secrets.GITHUB_TOKEN }}

    - name: Extract metadata
      id: meta
      uses: docker/metadata-action@v5
      with:
        images: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}
        tags: |
          type=ref,event=branch
          type=sha
          type=semver,pattern={{version}}

    - name: Build and push Identity Service
      uses: docker/build-push-action@v5
      with:
        context: .
        file: src/services/identity/src/IdentityService.Api/Dockerfile
        push: true
        tags: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}/identity-service:${{ github.sha }}
        labels: ${{ steps.meta.outputs.labels }}

    - name: Build and push Product Service
      uses: docker/build-push-action@v5
      with:
        context: .
        file: src/services/product/src/ProductService.Api/Dockerfile
        push: true
        tags: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}/product-service:${{ github.sha }}
        labels: ${{ steps.meta.outputs.labels }}

    - name: Build and push Order Service
      uses: docker/build-push-action@v5
      with:
        context: .
        file: src/services/order/src/OrderService.Api/Dockerfile
        push: true
        tags: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}/order-service:${{ github.sha }}
        labels: ${{ steps.meta.outputs.labels }}

    - name: Build and push Gateway Service
      uses: docker/build-push-action@v5
      with:
        context: .
        file: src/services/gateway/src/GatewayService.Api/Dockerfile
        push: true
        tags: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}/gateway-service:${{ github.sha }}
        labels: ${{ steps.meta.outputs.labels }}
```

> **Giải thích:**
> - **ci.yml**: Chạy mỗi khi có push/PR - build + test code
> - **docker.yml**: Chạy khi push lên main - build + push Docker images
> - **ghcr.io**: GitHub Container Registry - lưu images free cho public repos

---

## Bước 7.4: Build và Test Docker Compose

```bash
cd infrastructure
docker-compose up -d --build
```

> **Giải thích:**
> - Build tất cả images và chạy containers
> - Dùng `--build` để rebuild nếu đã có code changes

```bash
# Kiểm tra containers đang chạy
docker ps
```

> **Giải thích:**
> - Xem tất cả containers đang chạy

```bash
# Kiểm tra logs của một service
docker logs ecommerce_identity
```

> **Giải thích:**
> - Xem logs để debug nếu có vấn đề

```bash
# Test API qua Gateway
curl http://localhost:5000/health
curl http://localhost:5001/health
curl http://localhost:5002/health
curl http://localhost:5003/health
```

> **Giải thích:**
> - Test health endpoints của tất cả services

---

## ✅ CHECKLIST GIAI ĐOẠN 7

| Task | Mô tả | Status |
|------|-------|--------|
| 7.1 | Tạo Dockerfiles cho 4 services | ⬜ |
| 7.2 | Cập nhật docker-compose.yml | ⬜ |
| 7.3 | Tạo GitHub Actions workflows | ⬜ |
| 7.4 | Build và test Docker Compose | ⬜ |

---

## 🎉 HOÀN THÀNH!

Bạn đã triển khai xong hệ thống Microservice Ecommerce hoàn chỉnh!

### Kiến trúc tổng quan:

```
                    ┌──────────────┐
                    │   Gateway    │ :5000
                    │   (YARP)     │
                    └──────┬───────┘
                           │
        ┌──────────────────┼──────────────────┐
        │                  │                  │
   ┌────▼────┐      ┌────▼─────┐      ┌────▼──────┐
   │Identity │      │ Product  │      │   Order   │
   │ :5001   │      │  :5002   │      │   :5003    │
   └────┬────┘      └────┬──────┘      └────┬──────┘
        │               │                  │
        ▼               ▼                  ▼
   ┌─────────┐   ┌───────────┐   ┌─────────────┐
   │  PostgreSQL  │   │  PostgreSQL  │   │  PostgreSQL  │
   │  (identitydb)│   │  (productdb) │   │  (orderdb)   │
   └─────────┘   └───────────┘   └─────────────┘
                                              
                                        ┌─────────────┐
                                        │  RabbitMQ   │
                                        └─────────────┘
```

### Các components:

| Component | Technology |
|-----------|------------|
| API Gateway | YARP |
| Authentication | JWT + BCrypt |
| Database | PostgreSQL |
| Message Queue | RabbitMQ |
| Logging | Serilog |
| Service Communication | gRPC |
| CI/CD | GitHub Actions |
| Container | Docker + Docker Compose |

---

## ❓ CẦN HỖ TRỢ

Nếu gặp vấn đề ở bất kỳ bước nào, hãy reply:
> **"Need help: [vấn đề cụ thể]"**

Chúc bạn thành công! 🚀