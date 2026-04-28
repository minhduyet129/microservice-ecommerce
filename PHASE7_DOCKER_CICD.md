# 🎯 GIAI ĐOẠN 7: DOCKERIZATION & CI/CD
## Thời gian: Day 29-32
## Mục tiêu: Tạo Dockerfiles và CI/CD pipeline

---

## 📝 TASK 7.1: TẠO DOCKERFILES CHO TỪNG SERVICE

### Bước 7.1.1: Tạo Dockerfile cho IdentityService

Tạo file `src/services/identity/src/IdentityService.Api/Dockerfile`:

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

### Bước 7.1.2: Tạo Dockerfile cho ProductService

Tạo file `src/services/product/src/ProductService.Api/Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 5002

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

### Bước 7.1.3: Tạo Dockerfile cho OrderService

Tạo file `src/services/order/src/OrderService.Api/Dockerfile`:

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

### Bước 7.1.4: Tạo Dockerfile cho GatewayService

Tạo file `src/services/gateway/src/GatewayService.Api/Dockerfile`:

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

---

## 📝 TASK 7.2: UPDATE DOCKER COMPOSE

### Bước 7.2.1: Tạo docker-compose.yml đầy đủ

Cập nhật file `infrastructure/docker-compose.yml`:

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
    healthcheck:
      test: /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P YourStrong!Passw0rd -Q "SELECT 1" -C
      interval: 10s
      timeout: 3s
      retries: 5

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

  # Product Service
  product-service:
    build:
      context: ../..
      dockerfile: src/services/product/src/ProductService.Api/Dockerfile
    container_name: ecommerce_product
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=sqlserver,1433;Database=ProductDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True
    ports:
      - "5002:80"
    depends_on:
      sqlserver:
        condition: service_healthy
    networks:
      - ecommerce-network

  # Order Service
  order-service:
    build:
      context: ../..
      dockerfile: src/services/order/src/OrderService.Api/Dockerfile
    container_name: ecommerce_order
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=sqlserver,1433;Database=OrderDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True
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
      sqlserver:
        condition: service_healthy
      rabbitmq:
        condition: service_started
      product-service:
        condition: service_started
    networks:
      - ecommerce-network

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
      - ReverseProxy__Clusters__identityCluster__Destinations__identityService__Address=http://identity-service:80
      - ReverseProxy__Clusters__productCluster__Destinations__productService__Address=http://product-service:80
      - ReverseProxy__Clusters__orderCluster__Destinations__orderService__Address=http://order-service:80
    ports:
      - "5000:80"
    depends_on:
      - identity-service
      - product-service
      - order-service
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

## 📝 TASK 7.3: TẠO GITHUB ACTIONS WORKFLOW

### Bước 7.3.1: Tạo workflow directory

```bash
mkdir -p .github/workflows
```

### Bước 7.3.2: Tạo CI workflow

Tạo file `.github/workflows/ci.yml`:

```yaml
name: CI Build

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main, develop]

env:
  DOTNET_VERSION: '8.0.x'
  REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository }}

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout
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

  build-containers:
    needs: build
    runs-on: ubuntu-latest
    if: github.event_name == 'push'
    
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

    - name: Build and push Identity Service
      uses: docker/build-push-action@v5
      with:
        context: .
        file: src/services/identity/src/IdentityService.Api/Dockerfile
        push: true
        tags: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}/identity-service:${{ github.sha }}

    - name: Build and push Product Service
      uses: docker/build-push-action@v5
      with:
        context: .
        file: src/services/product/src/ProductService.Api/Dockerfile
        push: true
        tags: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}/product-service:${{ github.sha }}

    - name: Build and push Order Service
      uses: docker/build-push-action@v5
      with:
        context: .
        file: src/services/order/src/OrderService.Api/Dockerfile
        push: true
        tags: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}/order-service:${{ github.sha }}

    - name: Build and push Gateway Service
      uses: docker/build-push-action@v5
      with:
        context: .
        file: src/services/gateway/src/GatewayService.Api/Dockerfile
        push: true
        tags: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}/gateway-service:${{ github.sha }}
```

---

## 📝 TASK 7.4: TEST FULL STACK LOCAL

### Bước 7.4.1: Build all Docker images

```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce\infrastructure
docker-compose build
```

### Bước 7.4.2: Run all services

```bash
docker-compose up -d
```

### Bước 7.4.3: Verify services

```bash
docker ps
curl http://localhost:5000/health
curl http://localhost:5001/health
curl http://localhost:5002/health
curl http://localhost:5003/health
```

### Bước 7.4.4: Test API flow

1. Register: `POST http://localhost:5000/api/auth/register`
2. Login: `POST http://localhost:5000/api/auth/login`
3. Get products: `GET http://localhost:5000/products/api/products`
4. Create order: `POST http://localhost:5000/orders/api/orders` (with JWT)

---

## ✅ CHECKLIST GIAI ĐOẠN 7

| Task | Status | Ghi chú |
|------|--------|---------|
| 7.1.1 | ⬜ | Tạo Dockerfile IdentityService |
| 7.1.2 | ⬜ | Tạo Dockerfile ProductService |
| 7.1.3 | ⬜ | Tạo Dockerfile OrderService |
| 7.1.4 | ⬜ | Tạo Dockerfile GatewayService |
| 7.2.1 | ⬜ | Update docker-compose.yml |
| 7.3.1 | ⬜ | Tạo workflow directory |
| 7.3.2 | ⬜ | Tạo CI workflow |
| 7.4.1 | ⬜ | Build Docker images |
| 7.4.2 | ⬜ | Run all services |
| 7.4.3 | ⬜ | Verify services |
| 7.4.4 | ⬜ | Test API flow |

---

## 🎉 HOÀN THÀNH!

Bạn đã triển khai xong hệ thống Microservice Ecommerce đầy đủ:

- ✅ IdentityService - Authentication & JWT
- ✅ ProductService - Product CRUD + gRPC
- ✅ OrderService - Order + Saga + RabbitMQ + Polly
- ✅ GatewayService - YARP routing
- ✅ Docker Compose - Full stack local
- ✅ CI/CD - GitHub Actions

### 📚 TIẾP THEO ĐỀ XUẤT:

1. **Unit Tests** - Viết tests cho từng service
2. **Distributed Tracing** - Thêm Jaeger/Zipkin
3. **ELK Stack** - Centralized logging với Elasticsearch
4. **Kubernetes** - Deploy lên K8s với Helm charts

---

## ❓ CẦN HỖ TRỢ

Nếu gặp vấn đề ở bất kỳ task nào, hãy reply:
> **"Need help: [task cụ thể]"**

Tôi sẽ hỗ trợ chi tiết.