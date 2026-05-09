# AGENTS.md

## Project Overview
.NET 8 microservice ecommerce with 4 services: IdentityService, ProductService, OrderService, GatewayService (YARP).
Uses Clean Architecture (Domain/Application/Infrastructure/Api layers per service).

## Build Commands
```bash
dotnet build                    # Full solution
dotnet build src/services/identity/src/IdentityService.Api  # Per-service
dotnet test --no-build --verbosity normal  # Run tests
```

## Database Setup
1. Start infrastructure: `cd infrastructure && docker-compose up -d`
2. Create databases manually:
   ```bash
   docker exec ecommerce_postgres psql -U sa -d postgres -c "CREATE DATABASE identitydb;"
   docker exec ecommerce_postgres psql -U sa -d postgres -c "CREATE DATABASE productdb;"
   docker exec ecommerce_postgres psql -U sa -d postgres -c "CREATE DATABASE orderdb;"
   ```
3. Run migrations per service:
   ```bash
   dotnet ef database update --project src/services/identity/src/IdentityService.Infrastructure --startup-project src/services/identity/src/IdentityService.Api
   # Repeat for product and order services
   ```

## Service Ports (known inconsistency)
| Service | launchSettings.json | docker-compose |
|---------|---------------------|----------------|
| Identity | 5007 | 5001 |
| Product | 5002 | 5002 |
| Order | 5003 | 5003 |
| Gateway | 5000 | 5000 |

## Running Services
```bash
dotnet run --project src/services/identity/src/IdentityService.Api
dotnet run --project src/services/product/src/ProductService.Api
dotnet run --project src/services/order/src/OrderService.Api
dotnet run --project src/services/gateway/src/GatewayService.Api
```

## Important Patterns

### Domain Events
Domain events must implement `IDomainEvent` with `OccurredOn` property:
```csharp
public record OrderCreatedEvent(Guid OrderId) : IDomainEvent
{
    public DateTime OccurredOn => DateTime.UtcNow;
}
```

### Aggregate Roots
Orders must implement `IAggregateRoot` with `GetDomainEvents()` and `AddDomainEvent()`:
```csharp
public IReadOnlyList<IDomainEvent> GetDomainEvents() => _domainEvents.AsReadOnly();
public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
```

### gRPC Inter-service Communication
ProductService.gRPC uses `GrpcServices="Both"` in the .csproj for client/server code generation. OrderService.Application references this to call ProductService.

## Known Issues / Workarounds
- **MassTransit.AspNetCore**: Commented out in OrderService due to NuGet version conflicts (no stable 8.x release)
- **Grpc.Net.Client version conflict**: Add explicit `<PackageReference Include="Grpc.Net.Client" Version="2.57.0" />` to OrderService.Infrastructure if needed
- **EF Core migration --project flag**: Points to Infrastructure project (contains DbContext), --startup-project points to Api (contains connection string)

## Architecture Notes
- BuildingBlocks.Core/Shared = shared kernel referenced by all services
- No cross-service direct references - use gRPC via ProductService.gRPC
- OrderService calls ProductService via gRPC for stock checks
- Each service has its own PostgreSQL database
- RabbitMQ is included in docker-compose but MassTransit consumer code is commented out

## Database Connection String Format
PostgreSQL: `Host=localhost;Port=5432;Database=identitydb;Username=sa;Password=YourStrong!Passw0rd`

## Project Naming Convention
- `{ServiceName}.Domain`, `{ServiceName}.Application`, `{ServiceName}.Infrastructure`, `{ServiceName}.Api`
- gRPC projects: `{ServiceName}.gRPC`
- Tests: `{ServiceName}.UnitTests` (not yet created)