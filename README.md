# Microservice Ecommerce - Project Plan

## 📋 Tổng quan

---

## 🚀 BẮT ĐẦU

Đọc các file hướng dẫn chi tiết theo thứ tự:

| Phase | File | Mục tiêu |
|-------|------|-----------|
| Phase 1 | `PHASE1_FOUNDATION.md` | Solution structure + Docker infra |
| Phase 2 | `PHASE2_BUILDING_BLOCKS.md` | Shared kernel (Core abstractions) |
| Phase 3 | `PHASE3_IDENTITY_SERVICE.md` | Auth & JWT service |
| Phase 4 | `PHASE4_API_GATEWAY.md` | YARP API Gateway |
| Phase 5 | `PHASE5_PRODUCT_SERVICE.md` | Product service + gRPC |
| Phase 6 | `PHASE6_ORDER_SERVICE.md` | Order service + Saga + RabbitMQ + Polly |
| Phase 7 | `PHASE7_DOCKER_CICD.md` | Docker + CI/CD |

---

## 📋 Tổng quan

| Thành phần | Công nghệ |
|------------|-----------|
| API Gateway | YARP |
| Database | SQL Server |
| Message Queue | RabbitMQ |
| Logging | Serilog + Elasticsearch |
| Auth | JWT |
| Container | Docker Compose |

---

## 📁 Cấu trúc Project

```
MicroserviceEcommerce/
├── src/
│   ├── buildingblocks/                    # Shared kernel - reuse across services
│   │   ├── Core/
│   │   │   ├── Abstractions/              # IRepository, IUnitOfWork, IAggregateRoot
│   │   │   ├── Events/                    # IDomainEvent, EventDispatcher
│   │   │   ├── Exceptions/                # Custom exceptions
│   │   │   └── Extensions/                # Extension methods
│   │   └── Shared/
│   │       ├── DTOs/                      # PagedResult, ResponseWrapper
│   │       └── Enums/
│   │
│   ├── services/                          # Each service = 1 business capability
│   │   ├── identity/
│   │   │   ├── src/
│   │   │   │   ├── IdentityService.Api/           # Controllers, DTOs, Filters
│   │   │   │   ├── IdentityService.Application/  # Use Cases, Commands, Queries
│   │   │   │   ├── IdentityService.Domain/       # Entities, Value Objects, Interfaces
│   │   │   │   └── IdentityService.Infrastructure/ # EF Core, Repositories
│   │   │   └── tests/
│   │   │       └── IdentityService.UnitTests/
│   │   │
│   │   ├── product/
│   │   │   ├── src/
│   │   │   │   ├── ProductService.Api/
│   │   │   │   ├── ProductService.Application/
│   │   │   │   ├── ProductService.Domain/
│   │   │   │   ├── ProductService.Infrastructure/
│   │   │   │   └── ProductService.gRPC/          # gRPC for inter-service
│   │   │   └── tests/
│   │   │
│   │   ├── order/
│   │   │   ├── src/
│   │   │   │   ├── OrderService.Api/
│   │   │   │   ├── OrderService.Application/
│   │   │   │   ├── OrderService.Domain/
│   │   │   │   ├── OrderService.Infrastructure/
│   │   │   │   └── OrderService.gRPC/
│   │   │   └── tests/
│   │   │
│   │   └── gateway/
│   │       └── GatewayService.Api/
│   │
│   └── tests/                             # Shared test utilities
│       └── Shared.TestUtils/
│
├── infrastructure/
│   ├── docker-compose.yml                # Base infrastructure
│   ├── docker-compose.override.yml       # Local development
│   ├── databases/
│   │   └── migrations/
│   └── rabbitmq/
│       └── definitions.json
│
├── docs/
│   ├── architecture/
│   │   ├── c4-system-context.md
│   │   ├── c4-containers.md
│   │   └── api-contracts/
│   └── runbook/
│
└── scripts/
    ├── build.ps1
    └── migrate.ps1
```

---

## 🎯 Các Services

| Service | Responsibility | Database |
|---------|----------------|----------|
| IdentityService | Auth, JWT, User management | IdentityDb |
| ProductService | Product catalog, Inventory | ProductDb |
| OrderService | Order processing, Cart | OrderDb |
| GatewayService | API Routing, Auth entry | N/A |

---

## ✅ Giai đoạn & Tasks

### Giai đoạn 1: Foundation (Day 1-3)
- [ ] **Task 1.1:** Tạo Solution file `MicroserviceEcommerce.sln`
- [ ] **Task 1.2:** Tạo solution structure folders
- [ ] **Task 1.3:** Tạo Docker Compose với SQL Server + RabbitMQ

### Giai đoạn 2: Building Blocks (Day 4-6)
- [ ] **Task 2.1:** Tạo BuildingBlocks.Core project
- [ ] **Task 2.2:** Implement IRepository, IAggregateRoot
- [ ] **Task 2.3:** Implement PagedResult, ResponseWrapper
- [ ] **Task 2.4:** Setup Serilog infrastructure

### Giai đoạn 3: IdentityService (Day 7-12)
- [ ] **Task 3.1:** Tạo 4 layer projects (Api, Application, Domain, Infrastructure)
- [ ] **Task 3.2:** Setup Domain entities (User, Role)
- [ ] **Task 3.3:** Setup EF Core + DbContext
- [ ] **Task 3.4:** Implement Register/Login API
- [ ] **Task 3.5:** Implement JWT Authentication
- [ ] **Task 3.6:** Add Swagger + Health Checks
- [ ] **Task 3.7:** Viết Unit Tests

### Giai đoạn 4: API Gateway - YARP (Day 13-15)
- [ ] **Task 4.1:** Tạo GatewayService project
- [ ] **Task 4.2:** Config YARP routing
- [ ] **Task 4.3:** Add JWT validation middleware
- [ ] **Task 4.4:** Test routing to IdentityService

### Giai đoạn 5: ProductService (Day 16-21)
- [ ] **Task 5.1:** Tạo 4 layer projects
- [ ] **Task 5.2:** Setup Domain (Product, Category)
- [ ] **Task 5.3:** Setup EF Core DbContext
- [ ] **Task 5.4:** Implement Product CRUD
- [ ] **Task 5.5:** Add gRPC endpoint
- [ ] **Task 5.6:** Configure Serilog + Elasticsearch
- [ ] **Task 5.7:** Add to Gateway routes
- [ ] **Task 5.8:** Viết Unit Tests

### Giai đoạn 6: OrderService (Day 22-28)
- [ ] **Task 6.1:** Tạo 4 layer projects
- [ ] **Task 6.2:** Setup Domain (Order, OrderItem - Saga root)
- [ ] **Task 6.3:** Setup EF Core DbContext
- [ ] **Task 6.4:** Implement Order workflow
- [ ] **Task 6.5:** Call ProductService via gRPC
- [ ] **Task 6.6:** Setup RabbitMQ publisher (MassTransit)
- [ ] **Task 6.7:** Implement Saga pattern (Choreography)
- [ ] **Task 6.8:** Add Polly Circuit Breaker
- [ ] **Task 6.9:** Add to Gateway routes
- [ ] **Task 6.10:** Viết Unit Tests

### Giai đoạn 7: Dockerization & CI/CD (Day 29-32)
- [ ] **Task 7.1:** Tạo Dockerfiles cho từng service
- [ ] **Task 7.2:** Update docker-compose với all services
- [ ] **Task 7.3:** Tạo GitHub Actions workflow
- [ ] **Task 7.4:** Test full stack local

### Giai đoạn 8: Documentation (Day 33-35)
- [ ] **Task 8.1:** Generate C4 Architecture diagrams
- [ ] **Task 8.2:** Document API contracts
- [ ] **Task 8.3:** Tạo Runbook vận hành

---

## 🔧 Naming Conventions

| Layer | Suffix | Example |
|-------|--------|---------|
| Domain | Domain | IdentityService.Domain |
| Application | Application | IdentityService.Application |
| Infrastructure | Infrastructure | IdentityService.Infrastructure |
| API | Api | IdentityService.Api |
| gRPC | .gRPC | ProductService.gRPC |
| Tests | .UnitTests | IdentityService.UnitTests |

---

## 📦 Dependencies Management

Mỗi service reference:
- `BuildingBlocks.Core` (shared abstractions)
- Service's own Application, Domain, Infrastructure

Api layer references:
- Application layer
- Infrastructure layer

Không reference cross-service trực tiếp - dùng gRPC hoặc HTTP via Gateway.