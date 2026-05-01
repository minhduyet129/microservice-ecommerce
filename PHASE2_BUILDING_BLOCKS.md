# 🎯 GIAI ĐOẠN 2: BUILDING BLOCKS (SHARED KERNEL)

---

## Bước 2.1: Tạo BuildingBlocks.Core Project

```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce
dotnet new classlib -n BuildingBlocks.Core -o src/buildingblocks/Core
rm src/buildingblocks/Core/Class1.cs
dotnet sln add src/buildingblocks/Core/BuildingBlocks.Core.csproj
```

> **Giải thích:**
> - **Class library**: Project không chạy được, chỉ contain code dùng chung
> - **BuildingBlocks.Core**: Chứa abstractions (interfaces, base classes) dùng cho tất cả services
> - **Class1.cs**: File mặc định khi tạo classlib, xóa vì không cần
> - Thêm vào solution để quản lý

---

## Bước 2.2: Tạo BuildingBlocks.Shared Project

```bash
dotnet new classlib -n BuildingBlocks.Shared -o src/buildingblocks/Shared
rm src/buildingblocks/Shared/Class1.cs
dotnet sln add src/buildingblocks/Shared/BuildingBlocks.Shared.csproj
```

> **Giải thích:**
> - **BuildingBlocks.Shared**: Chứa DTOs (Data Transfer Objects) dùng chung như ResponseWrapper, PagedResult
> - Tách riêng vì Core sẽ reference Shared

---

## Bước 2.3: Add Reference (Shared → Core)

```bash
dotnet add src/buildingblocks/Core/BuildingBlocks.Core.csproj reference src/buildingblocks/Shared/BuildingBlocks.Shared.csproj
```

> **Giải thích:**
> - Core cần dùng Shared (DTOs)
> - Reference này cho phép code trong Core sử dụng classes từ Shared
> - Các services sau sẽ reference cả Core và Shared

---

## Bước 2.4: Update Target Framework

Mở `src/buildingblocks/Core/BuildingBlocks.Core.csproj` và sửa thành:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\Shared\BuildingBlocks.Shared.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Serilog" Version="4.0.0" />
  </ItemGroup>

</Project>
```

> **Giải thích:**
> - **TargetFramework**: .NET 8.0 (không phải .NET 10 vì chưa release)
> - **Serilog**: Logging library - dùng để log thay cho Console.WriteLine, log ra file, console, elasticsearch

Tương tự, mở `src/buildingblocks/Shared/BuildingBlocks.Shared.csproj` và sửa:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>
```

---

## Bước 2.5: Tạo IAggregateRoot Interface

Tạo file `src/buildingblocks/Core/Abstractions/IAggregateRoot.cs`:

```csharp
namespace BuildingBlocks.Core.Abstractions;

public interface IAggregateRoot
{
    IReadOnlyList<IDomainEvent> GetDomainEvents();
    void ClearDomainEvents();
}
```

> **Giải thích:**
> - **IAggregateRoot**: DDD (Domain-Driven Design) pattern - đánh dấu entity là aggregate root
> - **Aggregate Root**: Entry point để access tất cả related entities trong một aggregate
> - **GetDomainEvents**: Lấy danh sách domain events đã xảy ra (dùng cho Saga pattern)
> - Ví dụ: Order là aggregate root, OrderItems chỉ được access qua Order

---

## Bước 2.6: Tạo IRepository Interface

Tạo file `src/buildingblocks/Core/Abstractions/IRepository.cs`:

```csharp
using System.Linq.Expressions;

namespace BuildingBlocks.Core.Abstractions;

public interface IRepository<T> where T : IAggregateRoot
{
    Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default) where TId : notnull;
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
}
```

> **Giải thích:**
> - **IRepository**: Standard interface cho data access, tất cả services sẽ implement
> - **Generic type T**: Mỗi entity sẽ có repository riêng nhưng implement cùng interface
> - **Where T : IAggregateRoot**: Chỉ accept entities là aggregate roots
> - **Expression<Func<T, bool>>**: Cho phép filter phức tạp, dịch thành SQL WHERE clause

---

## Bước 2.7: Tạo Entity Base Class

Tạo file `src/buildingblocks/Core/Abstractions/Entity.cs`:

```csharp
using System.Collections.ObjectModel;

namespace BuildingBlocks.Core.Abstractions;

public abstract class Entity
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public string? CreatedBy { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public string? UpdatedBy { get; protected set; }

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

public abstract class Entity<TId> : Entity where TId : notnull
{
    public TId Id { get; protected set; } = default!;
}
```

> **Giải thích:**
> - **Entity**: Base class cho tất cả entities trong hệ thống
> - **Entity<TId>**: Generic version với Id có thể là Guid, int, string...
> - **Timestamps**: Tự động có CreatedAt, UpdatedAt cho audit trail
> - **Domain Events**: Support event-driven architecture (dùng cho Saga)

---

## Bước 2.8: Tạo IUnitOfWork Interface

Tạo file `src/buildingblocks/Core/Abstractions/IUnitOfWork.cs`:

```csharp
namespace BuildingBlocks.Core.Abstractions;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default);
}
```

> **Giải thích:**
> - **IUnitOfWork**: Transaction management, track tất cả changes và commit/rollback
> - Trong EF Core, DbContext đã là IUnitOfWork rồi, nên interface này có thể không cần dùng trực tiếp

---

## Bước 2.9: Tạo IDomainEvent Interface

Tạo file `src/buildingblocks/Core/Events/IDomainEvent.cs`:

```csharp
namespace BuildingBlocks.Core.Events;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
```

> **Giải thích:**
> - **IDomainEvent**: Đánh dấu một sự kiện quan trọng trong domain
> - **OccurredOn**: Timestamp khi event xảy ra
> - **Ví dụ**: OrderCreatedEvent, PaymentCompletedEvent - dùng để notify các services khác

---

## Bước 2.10: Tạo Custom Exceptions

Tạo file `src/buildingblocks/Core/Exceptions/NotFoundException.cs`:

```csharp
namespace BuildingBlocks.Core.Exceptions;

public class NotFoundException : Exception
{
    public string EntityName { get; set; }
    public object Key { get; set; }

    public NotFoundException(string entityName, object key)
        : base($"Entity '{entityName}' with key '{key}' was not found.")
    {
        EntityName = entityName;
        Key = key;
    }
}
```

> **Giải thích:**
> - **NotFoundException**: Ném khi query entity không tìm thấy
> - Thay vì trả null, ném exception để caller biết có lỗi

Tạo file `src/buildingblocks/Core/Exceptions/BadRequestException.cs`:

```csharp
namespace BuildingBlocks.Core.Exceptions;

public class BadRequestException : Exception
{
    public List<string>? Errors { get; set; }

    public BadRequestException(string message, List<string>? errors = null)
        : base(message)
    {
        Errors = errors;
    }
}
```

> **Giải thích:**
> - **BadRequestException**: Ném khi validation fail hoặc business rule violated
> - **Errors list**: Chứa danh sách lỗi chi tiết để trả về client

---

## Bước 2.11: Tạo PagedResult DTO

Tạo file `src/buildingblocks/Shared/DTOs/PagedResult.cs`:

```csharp
namespace BuildingBlocks.Shared.DTOs;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;

    public static PagedResult<T> Empty(int pageNumber = 1, int pageSize = 10) => new()
    {
        PageNumber = pageNumber,
        PageSize = pageSize
    };

    public static PagedResult<T> Create(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
    {
        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
```

> **Giải thích:**
> - **PagedResult**: Standard response cho APIs trả danh sách có phân trang
> - **Items**: Dữ liệu trang hiện tại
> - **TotalCount**: Tổng số records (để tính tổng pages)
> - **PageNumber/PageSize**: Thông tin phân trang

---

## Bước 2.12: Tạo ResponseWrapper DTO

Tạo file `src/buildingblocks/Shared/DTOs/ResponseWrapper.cs`:

```csharp
namespace BuildingBlocks.Shared.DTOs;

public class ResponseWrapper<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ResponseWrapper<T> Success(T data, string? message = null) => new()
    {
        IsSuccess = true,
        Data = data,
        Message = message
    };

    public static ResponseWrapper<T> Fail(string message, List<string>? errors = null) => new()
    {
        IsSuccess = false,
        Message = message,
        Errors = errors
    };
}

public class ResponseWrapper
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ResponseWrapper Success(string? message = null) => new()
    {
        IsSuccess = true,
        Message = message
    };

    public static ResponseWrapper Fail(string message, List<string>? errors = null) => new()
    {
        IsSuccess = false,
        Message = message,
        Errors = errors
    };
}
```

> **Giải thích:**
> - **ResponseWrapper**: Standard wrapper cho tất cả API responses
> - **IsSuccess**: Client biết request thành công hay không
> - **Data**: Dữ liệu trả về (generic type)
> - **Errors**: Danh sách lỗi nếu có
> - **Timestamp**: Giúp debug, trace request

---

## Bước 2.13: Build Projects

```bash
dotnet build src/buildingblocks/Core/BuildingBlocks.Core.csproj
dotnet build src/buildingblocks/Shared/BuildingBlocks.Shared.csproj
```

> **Giải thích:**
> - Kiểm tra code compile không có lỗi
> - Nếu có lỗi, đọc message và fix

---

## ✅ CHECKLIST GIAI ĐOẠN 2

| Task | Mô tả | Status |
|------|-------|--------|
| 2.1 | Tạo BuildingBlocks.Core project | ⬜ |
| 2.2 | Tạo BuildingBlocks.Shared project | ⬜ |
| 2.3 | Add reference Core → Shared | ⬜ |
| 2.4 | Update target framework (net8.0) | ⬜ |
| 2.5 | Tạo IAggregateRoot | ⬜ |
| 2.6 | Tạo IRepository | ⬜ |
| 2.7 | Tạo Entity base class | ⬜ |
| 2.8 | Tạo IUnitOfWork | ⬜ |
| 2.9 | Tạo IDomainEvent | ⬜ |
| 2.10 | Tạo Custom Exceptions | ⬜ |
| 2.11 | Tạo PagedResult | ⬜ |
| 2.12 | Tạo ResponseWrapper | ⬜ |
| 2.13 | Build and verify | ⬜ |

---

## ❓ KHI HOÀN THÀNH

Khi tất cả tasks hoàn thành, reply:
> **"Done Phase 2"**

Tôi sẽ hướng dẫn tiếp **Giai đoạn 3: Identity Service**