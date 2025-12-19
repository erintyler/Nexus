namespace Nexus.Domain.Primitives;

public interface IDomainEvent
{
    Guid Id { get; init; }
}