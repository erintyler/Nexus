namespace Nexus.Domain.Events;

public interface INexusEvent
{
    string EventName { get; }
    string Description { get; }
}