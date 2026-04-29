namespace BuildingBlocks.Core.Events;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}