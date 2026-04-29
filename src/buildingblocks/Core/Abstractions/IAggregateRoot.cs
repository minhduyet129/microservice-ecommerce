using BuildingBlocks.Core.Events;

namespace BuildingBlocks.Core.Abstractions;

public interface IAggregateRoot
{
    IReadOnlyList<IDomainEvent> GetDomainEvents();
    void ClearDomainEvents();
}