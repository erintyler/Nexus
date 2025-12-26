using Marten.Events.Aggregation;
using Nexus.Domain.Entities;
using Nexus.Domain.Events.Users;

namespace Nexus.Application.Features.Users.Common.Projections;

/// <summary>
/// Inline projection that maintains User as a queryable document.
/// This allows efficient lookups by DiscordId without reconstructing from events.
/// The projection leverages the User entity's Apply methods for event handling,
/// following the same pattern as other event-sourced aggregates in the domain.
/// </summary>
public class UserProjection : SingleStreamProjection<User, Guid>
{
    // Marten automatically calls the User.Apply(UserCreatedDomainEvent) method
    // defined on the User aggregate. This keeps the projection logic consistent
    // with the aggregate's own event application logic.
}
