# 🎯 GIAI ĐOẠN 7B: FIX CÁC MỤC CHƯA HOÀN THÀNH (Phần 2)

---

## Mục tiêu Phase này:
1. ✅ Tạo Unit Tests projects
2. ✅ Cập nhật Serilog (thêm file logging)
3. ✅ Cập nhật README.md (SQL Server → PostgreSQL)

---

## Bước 7B-1: Tạo Unit Tests Projects

### Mục đích:
- Tạo test projects cho mỗi service
- Viết sample unit tests cho Domain và Application layers

### Cách làm:

**1. Tạo IdentityService.UnitTests:**

```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce
dotnet new xunit -n IdentityService.UnitTests -o src/tests/IdentityService.UnitTests
dotnet sln add src/tests/IdentityService.UnitTests/IdentityService.UnitTests.csproj
```

**Thêm references:**

```bash
dotnet add src/tests/IdentityService.UnitTests/IdentityService.UnitTests.csproj reference src/services/identity/src/IdentityService.Domain/IdentityService.Domain.csproj
dotnet add src/tests/IdentityService.UnitTests/IdentityService.UnitTests.csproj reference src/services/identity/src/IdentityService.Application/IdentityService.Application.csproj
```

**2. Tạo ProductService.UnitTests:**

```bash
dotnet new xunit -n ProductService.UnitTests -o src/tests/ProductService.UnitTests
dotnet sln add src/tests/ProductService.UnitTests/ProductService.UnitTests.csproj
```

**Thêm references:**

```bash
dotnet add src/tests/ProductService.UnitTests/ProductService.UnitTests.csproj reference src/services/product/src/ProductService.Domain/ProductService.Domain.csproj
dotnet add src/tests/ProductService.UnitTests/ProductService.UnitTests.csproj reference src/services/product/src/ProductService.Application/ProductService.Application.csproj
```

**3. Tạo OrderService.UnitTests:**

```bash
dotnet new xunit -n OrderService.UnitTests -o src/tests/OrderService.UnitTests
dotnet sln add src/tests/OrderService.UnitTests/OrderService.UnitTests.csproj
```

**Thêm references:**

```bash
dotnet add src/tests/OrderService.UnitTests/OrderService.UnitTests.csproj reference src/services/order/src/OrderService.Domain/OrderService.Domain.csproj
dotnet add src/tests/OrderService.UnitTests/OrderService.UnitTests.csproj reference src/services/order/src/OrderService.Application/OrderService.Application.csproj
```

**4. Thêm Moq package:**

```bash
dotnet add src/tests/IdentityService.UnitTests/IdentityService.UnitTests.csproj package Moq --version 4.20.70
dotnet add src/tests/ProductService.UnitTests/ProductService.UnitTests.csproj package Moq --version 4.20.70
dotnet add src/tests/OrderService.UnitTests/OrderService.UnitTests.csproj package Moq --version 4.20.70
```

**5. Xóa file test mặc định và tạo sample tests:**

Tạo file `src/tests/IdentityService.UnitTests/Domain/Entities/UserTests.cs`:

```csharp
using IdentityService.Domain.Entities;

namespace IdentityService.UnitTests.Domain.Entities;

public class UserTests
{
    [Fact]
    public void CreateUser_WithValidParameters_ShouldCreateSuccessfully()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password123";

        // Act
        var user = User.Create(email, password);

        // Assert
        Assert.NotNull(user);
        Assert.Equal(email, user.Email);
        Assert.NotNull(user.PasswordHash);
        Assert.True(user.Id != Guid.Empty);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void CreateUser_WithInvalidEmail_ShouldThrowException(string? email)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => User.Create(email!, "password123"));
    }

    [Fact]
    public void User_VerifyPassword_ShouldReturnTrueForCorrectPassword()
    {
        // Arrange
        var user = User.Create("test@example.com", "password123");

        // Act
        var result = user.VerifyPassword("password123");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void User_VerifyPassword_ShouldReturnFalseForWrongPassword()
    {
        // Arrange
        var user = User.Create("test@example.com", "password123");

        // Act
        var result = user.VerifyPassword("wrongpassword");

        // Assert
        Assert.False(result);
    }
}
```

Tạo file `src/tests/ProductService.UnitTests/Domain/Entities/ProductTests.cs`:

```csharp
using ProductService.Domain.Entities;

namespace ProductService.UnitTests.Domain.Entities;

public class ProductTests
{
    [Fact]
    public void CreateProduct_WithValidParameters_ShouldCreateSuccessfully()
    {
        // Arrange
        var name = "Test Product";
        var price = 100.00m;
        var stock = 10;
        var categoryId = Guid.NewGuid();

        // Act
        var product = Product.Create(name, "Description", price, stock, categoryId);

        // Assert
        Assert.NotNull(product);
        Assert.Equal(name, product.Name);
        Assert.Equal(price, product.Price);
        Assert.Equal(stock, product.StockQuantity);
        Assert.NotNull(product.SKU);
    }

    [Fact]
    public void Product_UpdateStock_ShouldIncreaseStock()
    {
        // Arrange
        var product = Product.Create("Test", "Desc", 100, 10, Guid.NewGuid());
        var initialStock = product.StockQuantity;

        // Act
        product.UpdateStock(5);

        // Assert
        Assert.Equal(initialStock + 5, product.StockQuantity);
    }

    [Fact]
    public void Product_UpdateStock_NegativeValue_ShouldDecreaseStock()
    {
        // Arrange
        var product = Product.Create("Test", "Desc", 100, 10, Guid.NewGuid());

        // Act
        product.UpdateStock(-3);

        // Assert
        Assert.Equal(7, product.StockQuantity);
    }

    [Fact]
    public void Product_UpdateStock_NegativeResult_ShouldSetToZero()
    {
        // Arrange
        var product = Product.Create("Test", "Desc", 100, 5, Guid.NewGuid());

        // Act
        product.UpdateStock(-10);

        // Assert
        Assert.Equal(0, product.StockQuantity);
    }

    [Fact]
    public void Product_Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var product = Product.Create("Test", "Desc", 100, 10, Guid.NewGuid());

        // Act
        product.Deactivate();

        // Assert
        Assert.False(product.IsActive);
    }
}
```

Tạo file `src/tests/OrderService.UnitTests/Domain/Entities/OrderTests.cs`:

```csharp
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.UnitTests.Domain.Entities;

public class OrderTests
{
    [Fact]
    public void CreateOrder_WithValidParameters_ShouldCreateSuccessfully()
    {
        // Arrange
        var userId = "user123";
        var userEmail = "user@example.com";
        var address = "123 Main St";
        var phone = "1234567890";

        // Act
        var order = Order.Create(userId, userEmail, address, phone);

        // Assert
        Assert.NotNull(order);
        Assert.Equal(userId, order.UserId);
        Assert.Equal(userEmail, order.UserEmail);
        Assert.Equal(address, order.ShippingAddress);
        Assert.Equal(phone, order.ShippingPhone);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Equal(PaymentStatus.Pending, order.PaymentStatus);
    }

    [Fact]
    public void Order_AddItem_ShouldAddItemAndRecalculateTotal()
    {
        // Arrange
        var order = Order.Create("user123", "user@example.com", "Address", "Phone");

        // Act
        order.AddItem(Guid.NewGuid(), "Product 1", 100, 2);
        order.AddItem(Guid.NewGuid(), "Product 2", 50, 1);

        // Assert
        Assert.Equal(2, order.Items.Count);
        Assert.Equal(250, order.TotalAmount); // (100*2) + (50*1)
    }

    [Fact]
    public void Order_Confirm_ShouldChangeStatusToConfirmed()
    {
        // Arrange
        var order = Order.Create("user123", "user@example.com", "Address", "Phone");

        // Act
        order.Confirm();

        // Assert
        Assert.Equal(OrderStatus.Confirmed, order.Status);
        Assert.NotNull(order.UpdatedAt);
    }

    [Fact]
    public void Order_Cancel_ShouldChangeStatusToCancelled()
    {
        // Arrange
        var order = Order.Create("user123", "user@example.com", "Address", "Phone");

        // Act
        order.Cancel("Customer request");

        // Assert
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Equal("Customer request", order.DomainEvents.OfType<OrderCancelledEvent>().First().Reason);
    }

    [Fact]
    public void Order_MarkAsPaid_ShouldChangePaymentStatusAndStatus()
    {
        // Arrange
        var order = Order.Create("user123", "user@example.com", "Address", "Phone");

        // Act
        order.MarkAsPaid();

        // Assert
        Assert.Equal(PaymentStatus.Paid, order.PaymentStatus);
        Assert.Equal(OrderStatus.Processing, order.Status);
    }

    [Fact]
    public void Order_GetDomainEvents_ShouldReturnEvents()
    {
        // Arrange
        var order = Order.Create("user123", "user@example.com", "Address", "Phone");

        // Act
        var events = order.GetDomainEvents();

        // Assert
        Assert.Single(events);
        Assert.IsType<OrderCreatedEvent>(events.First());
    }

    [Fact]
    public void Order_ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        var order = Order.Create("user123", "user@example.com", "Address", "Phone");

        // Act
        order.ClearDomainEvents();
        var events = order.GetDomainEvents();

        // Assert
        Assert.Empty(events);
    }
}
```

> **Giải thích:**
> - Dùng xUnit framework
> - Dùng Moq để mock dependencies khi cần
> - Test Domain entities - nơi có business logic quan trọng nhất

---

## Bước 7B-2: Cập nhật Serilog (Thêm file logging)

### Mục đích:
- Thêm file sink để lưu logs ra file
- Giúp debug production issues

### Cách làm:

**1. Thêm Serilog.Sinks.File package:**

```bash
# Cho tất cả API projects
dotnet add src/services/identity/src/IdentityService.Api/IdentityService.Api.csproj package Serilog.Sinks.File --version 5.0.0
dotnet add src/services/product/src/ProductService.Api/ProductService.Api.csproj package Serilog.Sinks.File --version 5.0.0
dotnet add src/services/order/src/OrderService.Api/OrderService.Api.csproj package Serilog.Sinks.File --version 5.0.0
dotnet add src/services/gateway/src/GatewayService.Api/GatewayService.Api.csproj package Serilog.Sinks.File --version 5.0.0
```

**2. Cập nhật appsettings.json cho mỗi service:**

Ví dụ với IdentityService.Api - mở `src/services/identity/src/IdentityService.Api/appsettings.json`:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "/var/log/identity-service/log-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7,
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
  }
}
```

> **Giải thích:**
> - `rollingInterval: "Day"` - Tạo file mỗi ngày
> - `retainedFileCountLimit: 7` - Giữ 7 ngày
> - Trong Docker, logs sẽ ra `/var/log/...` - cần mount volume để lưu

**3. Thêm docker volume cho logs trong docker-compose:**

Trong `infrastructure/docker-compose.yml`, thêm cho mỗi service:

```yaml
identity-service:
  # ... existing config ...
  volumes:
    - identity_logs:/var/log

# Thêm vào cuối file
volumes:
  # ... existing volumes ...
  identity_logs:
```

> **Giải thích:**
> - Mount volume để logs không bị mất khi container restart

---

## Bước 7B-3: Cập nhật README.md

### Mục đích:
- Cập nhật thông tin database từ SQL Server → PostgreSQL
- Cập nhật checklist các tasks đã hoàn thành

### Cách làm:

Mở file `README.md` và thực hiện các thay đổi sau:

**1. Cập nhật phần Database:**

```diff
| Database | SQL Server | PostgreSQL |
```

**2. Cập nhật docker-compose path:**

```diff
- │   └── databases/
- │       └── migrations/
```

**3. Cập nhật checklist - đánh dấu đã hoàn thành:**

```
### Giai đoạn 1: Foundation (Day 1-3)
- [x] **Task 1.1:** Tạo Solution file `MicroserviceEcommerce.sln`
- [x] **Task 1.2:** Tạo solution structure folders
- [x] **Task 1.3:** Tạo Docker Compose với PostgreSQL + RabbitMQ

### Giai đoạn 2: Building Blocks (Day 4-6)
- [x] **Task 2.1:** Tạo BuildingBlocks.Core project
- [x] **Task 2.2:** Implement IRepository, IAggregateRoot
- [x] **Task 2.3:** Implement PagedResult, ResponseWrapper
- [x] **Task 2.4:** Setup Serilog infrastructure

### Giai đoạn 3: IdentityService (Day 7-12)
- [x] **Task 3.1:** Tạo 4 layer projects (Api, Application, Domain, Infrastructure)
- [x] **Task 3.2:** Setup Domain entities (User, Role)
- [x] **Task 3.3:** Setup EF Core + DbContext
- [x] **Task 3.4:** Implement Register/Login API
- [x] **Task 3.5:** Implement JWT Authentication
- [x] **Task 3.6:** Add Swagger + Health Checks
- [ ] **Task 3.7:** Viết Unit Tests (đã làm trong 7B-1)

### Giai đoạn 4: API Gateway - YARP (Day 13-15)
- [x] **Task 4.1:** Tạo GatewayService project
- [x] **Task 4.2:** Config YARP routing
- [x] **Task 4.3:** Add JWT validation middleware
- [x] **Task 4.4:** Test routing to IdentityService

### Giai đoạn 5: ProductService (Day 16-21)
- [x] **Task 5.1:** Tạo 5 layer projects (thêm gRPC)
- [x] **Task 5.2:** Setup Domain (Product, Category)
- [x] **Task 5.3:** Setup EF Core DbContext
- [x] **Task 5.4:** Implement Product CRUD
- [x] **Task 5.5:** Add gRPC endpoint
- [ ] **Task 5.6:** Configure Serilog + Elasticsearch (file logging trong 7B-2)
- [x] **Task 5.7:** Add to Gateway routes (đã fix trong 7A-1)
- [ ] **Task 5.8:** Viết Unit Tests (đã làm trong 7B-1)

### Giai đoạn 6: OrderService (Day 22-28)
- [x] **Task 6.1:** Tạo 4 layer projects
- [x] **Task 6.2:** Setup Domain (Order, OrderItem - Saga root)
- [x] **Task 6.3:** Setup EF Core DbContext
- [x] **Task 6.4:** Implement Order workflow
- [x] **Task 6.5:** Call ProductService via gRPC
- [x] **Task 6.6:** Setup RabbitMQ publisher (MassTransit 7.1.5)
- [x] **Task 6.7:** Implement Saga pattern (Choreography)
- [x] **Task 6.8:** Add Polly Circuit Breaker (đã thêm trong 7A-3)
- [x] **Task 6.9:** Add to Gateway routes (đã fix trong 7A-1)
- [ ] **Task 6.10:** Viết Unit Tests (đã làm trong 7B-1)

### Giai đoạn 7: Dockerization & CI/CD (Day 29-32)
- [ ] **Task 7.1:** Tạo Dockerfiles cho từng service
- [ ] **Task 7.2:** Update docker-compose với all services
- [ ] **Task 7.3:** Tạo GitHub Actions workflow
- [ ] **Task 7.4:** Test full stack local
```

> **Giải thích:**
> - Đánh dấu [x] các tasks đã hoàn thành
> - [ ] các tasks chưa làm hoặc đã làm ở phase khác

---

## Bước 7B-4: Build và chạy tests

```bash
# Build tất cả test projects
dotnet build src/tests

# Chạy tests
dotnet test src/tests --no-build
```

> **Giải thích:**
> - Build trước để đảm bảo không có lỗi
> - Chạy tests để xem kết quả

---

## ✅ CHECKLIST GIAI ĐOẠN 7B

| Task | Mô tả | Status |
|------|-------|--------|
| 7B-1 | Tạo Unit Tests (3 projects) | ⬜ |
| 7B-2 | Thêm Serilog file sink | ⬜ |
| 7B-3 | Update README.md | ⬜ |
| 7B-4 | Build và test verification | ⬜ |

---

## ❓ KHI HOÀN THÀNH

Reply: **"Done Phase 7B"**

Tôi sẽ hướng dẫn tiếp **Giai đoạn 7: Docker + CI/CD**