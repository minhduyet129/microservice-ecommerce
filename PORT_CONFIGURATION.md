# Port Configuration & Architecture Summary

## 📋 Overview

This document explains the current port configuration and service architecture of the Microservice Ecommerce system.

## 🔢 Port Configuration

| Service | Type | Port | Protocol | Purpose |
|---------|------|------|----------|---------|
| GatewayService | REST API | 5000 | HTTP/HTTPS | API Gateway (YARP) - Entry point for all client requests |
| IdentityService | REST API | 5001 | HTTP/HTTPS | Authentication & JWT token generation |
| ProductService | REST API | 5002 | HTTP/HTTPS | Product catalog management (CRUD operations) |
| OrderService | REST API | 5003 | HTTP/HTTPS | Order processing & management |
| ProductService | gRPC | 5004 | HTTP/2 | Inter-service communication (OrderService → ProductService) |

## 🏗️ Architecture Flow

```
Client (Browser/Mobile App)
         ↓
    Gateway (5000)
         ↓
    ┌────┴────┬──────────────┬──────────────┐
    ↓         ↓              ↓              ↓
Identity   Product        Order         Product
(5001)     (5002)         (5003)         gRPC
                                         (5004)
```

## 🔄 Inter-Service Communication

### OrderService → ProductService (gRPC)
- **Protocol**: gRPC over HTTP/2
- **Port**: 5004
- **Purpose**: OrderService calls ProductService to:
  - Check product availability
  - Get product details
  - Validate stock quantity
- **Configuration**: `OrderService.Api/appsettings.json`
  ```json
  "GrpcSettings": {
    "ProductServiceUrl": "http://localhost:5004"
  }
  ```

### Client → Services (via Gateway)
- **Protocol**: REST over HTTP/HTTPS
- **Port**: 5000 (Gateway)
- **Purpose**: All client requests go through Gateway for:
  - Centralized routing
  - JWT validation
  - Load balancing (in production)

## 📝 Configuration Files

### Gateway Configuration
- **File**: `src/services/gateway/src/GatewayService.Api/appsettings.json`
- **Routes**:
  - `/api/auth/*` → IdentityService (5001)
  - `/api/products/*` → ProductService (5002)
  - `/api/orders/*` → OrderService (5003)

### Service Launch Settings
- **IdentityService**: `src/services/identity/src/IdentityService.Api/Properties/launchSettings.json`
- **ProductService (REST)**: `src/services/product/src/ProductService.Api/Properties/launchSettings.json`
- **ProductService (gRPC)**: `src/services/product/src/ProductService.gRPC/Properties/launchSettings.json`
- **OrderService**: `src/services/order/src/OrderService.Api/Properties/launchSettings.json`
- **GatewayService**: `src/services/gateway/src/GatewayService.Api/Properties/launchSettings.json`

## 🚀 Starting All Services

To start all 5 services, open 5 separate terminals:

```bash
# Terminal 1 - IdentityService
dotnet run --project src/services/identity/src/IdentityService.Api

# Terminal 2 - ProductService (REST)
dotnet run --project src/services/product/src/ProductService.Api

# Terminal 3 - ProductService (gRPC)
dotnet run --project src/services/product/src/ProductService.gRPC

# Terminal 4 - OrderService
dotnet run --project src/services/order/src/OrderService.Api

# Terminal 5 - GatewayService
dotnet run --project src/services/gateway/src/GatewayService.Api
```

## ✅ Verification

Check all services are running:

```bash
netstat -ano | Select-String "LISTENING" | Select-String "500"
```

Expected output: 5 ports listening (5000, 5001, 5002, 5003, 5004)

## 🧪 Testing

Use `SYSTEM_TEST_GUIDE.md` for complete testing instructions:

1. Register user via IdentityService (5001)
2. Login to get JWT token
3. Create product via ProductService (5002)
4. Create order via OrderService (5003) - internally calls ProductService.gRPC (5004)
5. All requests can also go through Gateway (5000)

## 🔍 Key Points

1. **Separate Ports for REST and gRPC**: ProductService runs on two ports:
   - 5002 for REST API (client-facing)
   - 5004 for gRPC (inter-service communication)

2. **Gateway as Entry Point**: Clients should only interact with Gateway (5000), not individual services

3. **gRPC for Internal Communication**: OrderService uses gRPC to communicate with ProductService for better performance and type safety

4. **Local Development Configuration**: All services use `localhost` for local development. In production, this would be replaced with service discovery or DNS names.

## 📚 Related Documentation

- `SYSTEM_TEST_GUIDE.md` - Complete testing instructions
- `AGENTS.md` - Build commands and known issues
- `DEPLOYMENT_HEALTH_CHECKS.md` - Health check implementation guide
- Phase documentation files (PHASE1-8) - Detailed implementation guides
