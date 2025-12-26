using Marten.Events.Aggregation;
using Nexus.Domain.Entities;
using Nexus.Domain.Events.Users;

namespace Nexus.Application.Features.Users.Common.Projections;

/// <summary>
/// Inline projection that maintains User as a queryable document.
/// This allows efficient lookups by DiscordId without reconstructing from events.
/// Since User is an event-sourced aggregate, we let Marten handle instantiation
/// and just apply the events to update the projected state.
/// </summary>
public class UserProjection : SingleStreamProjection<User, Guid>
{
}
