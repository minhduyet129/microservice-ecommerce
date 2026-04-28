# 🎯 GIAI ĐOẠN 2: BUILDING BLOCKS
## Thời gian: Day 4-6
## Mục tiêu: Tạo shared kernel cho tất cả services

---

## 📝 TASK 2.1: TẠO BUILDINGBLOCKS.CORE PROJECT

### Bước 2.1.1: Tạo class library project

```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce
dotnet new classlib -n BuildingBlocks.Core -o src/buildingblocks/Core
```

### Bước 2.1.2: Add project vào solution

```bash
dotnet sln add src/buildingblocks/Core/BuildingBlocks.Core.csproj
```

### Bước 2.1.3: Xóa default Class1.cs

```bash
rm src/buildingblocks/Core/Class1.cs
```

---

## 📝 TASK 2.2: IMPLEMENT IAggregATE ROOT VÀ IREPOSITORY

### Bước 2.2.1: Tạo thư mục Abstractions

Đảm bảo thư mục `src/buildingblocks/Core/Abstractions/` đã tồn tại

### Bước 2.2.2: Tạo IAggregateRoot.cs

Tạo file `src/buildingblocks/Core/Abstractions/IAggregateRoot.cs`:

```csharp
namespace BuildingBlocks.Core.Abstractions;

public interface IAggregateRoot
{
    IReadOnlyList<IDomainEvent> GetDomainEvents();
    void ClearDomainEvents();
}
```

### Bước 2.2.3: Tạo IRepository.cs

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

### Bước 2.2.4: Tạo IUnitOfWork.cs

Tạo file `src/buildingblocks/Core/Abstractions/IUnitOfWork.cs`:

```csharp
namespace BuildingBlocks.Core.Abstractions;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default);
}
```

---

## 📝 TASK 2.3: IMPLEMENT BASE ENTITY VÀ EVENTS

### Bước 2.3.1: Tạo Entity.cs

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

### Bước 2.3.2: Tạo IDomainEvent.cs

Tạo file `src/buildingblocks/Core/Events/IDomainEvent.cs`:

```csharp
namespace BuildingBlocks.Core.Events;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
```

---

## 📝 TASK 2.4: IMPLEMENT PAGED RESULT VÀ RESPONSE WRAPPER

### Bước 2.4.1: Tạo PagedResult.cs

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

### Bước 2.4.2: Tạo ResponseWrapper.cs

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

---

## 📝 TASK 2.5: SETUP SERILOG INFRASTRUCTURE

### Bước 2.5.1: Tạo LoggingExtensions.cs

Tạo file `src/buildingblocks/Core/Extensions/LoggingExtensions.cs`:

```csharp
using Serilog;
using Serilog.Events;

namespace BuildingBlocks.Core.Extensions;

public static class LoggingExtensions
{
    public static LoggerConfiguration AddCustomLogging(this LoggerConfiguration loggerConfiguration, string serviceName)
    {
        return loggerConfiguration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.WithProperty("ServiceName", serviceName)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName();
    }
}
```

---

## 📝 TASK 2.6: IMPLEMENT CUSTOM EXCEPTIONS

### Bước 2.6.1: Tạo NotFoundException.cs

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

### Bước 2.6.2: Tạo BadRequestException.cs

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

---

## 📝 TASK 2.7: CREATE SHARED PROJECT

### Bước 2.7.1: Tạo BuildingBlocks.Shared project

```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce
dotnet new classlib -n BuildingBlocks.Shared -o src/buildingblocks/Shared
dotnet sln add src/buildingblocks/Shared/BuildingBlocks.Shared.csproj
rm src/buildingblocks/Shared/Class1.cs
```

### Bước 2.7.2: Add reference vào Core

```bash
dotnet add src/buildingblocks/Core/BuildingBlocks.Core.csproj reference src/buildingblocks/Shared/BuildingBlocks.Shared.csproj
```

---

## ✅ CHECKLIST GIAI ĐOẠN 2

| Task | Status | Ghi chú |
|------|--------|---------|
| 2.1.1 | ⬜ | Tạo BuildingBlocks.Core project |
| 2.1.2 | ⬜ | Add vào solution |
| 2.1.3 | ⬜ | Xóa Class1.cs |
| 2.2.1 | ⬜ | Tạo thư mục Abstractions |
| 2.2.2 | ⬜ | Tạo IAggregateRoot |
| 2.2.3 | ⬜ | Tạo IRepository |
| 2.2.4 | ⬜ | Tạo IUnitOfWork |
| 2.3.1 | ⬜ | Tạo Entity base class |
| 2.3.2 | ⬜ | Tạo IDomainEvent |
| 2.4.1 | ⬜ | Tạo PagedResult |
| 2.4.2 | ⬜ | Tạo ResponseWrapper |
| 2.5.1 | ⬜ | Tạo LoggingExtensions |
| 2.6.1 | ⬜ | Tạo NotFoundException |
| 2.6.2 | ⬜ | Tạo BadRequestException |
| 2.7.1 | ⬜ | Tạo BuildingBlocks.Shared |
| 2.7.2 | ⬜ | Add reference |

---

## ❓ KHI HOÀN THÀNH

Khi tất cả tasks hoàn thành, reply:
> **"Done Phase 2"**

Tôi sẽ hướng dẫn tiếp **Giai đoạn 3: IdentityService**