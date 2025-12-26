using Marten;
using Nexus.Application.Common.Abstractions;
using Nexus.Domain.Entities;
using Nexus.Domain.Events.Users;

namespace Nexus.Infrastructure.Repositories;

/// <summary>
/// Marten-based implementation of IUserRepository.
/// Provides data access for user operations using Marten event sourcing.
/// </summary>
public class UserRepository(IDocumentSession session) : IUserRepository
{
    public async Task<User?> GetByDiscordIdAsync(string discordId, CancellationToken ct)
    {
        return await session
            .Query<User>()
            .FirstOrDefaultAsync(u => u.DiscordId == discordId, ct);
    }

    public async Task<User?> GetByIdAsync(Guid userId, CancellationToken ct)
    {
        return await session.Events.AggregateStreamAsync<User>(userId, token: ct);
    }

    public async Task<Guid> CreateAsync(string discordId, string discordUsername, CancellationToken ct)
    {
        var userId = Guid.NewGuid();
        var createEvent = new UserCreatedDomainEvent(discordId, discordUsername);
        
        session.Events.StartStream<User>(userId, createEvent);
        await session.SaveChangesAsync(ct);
        
        return userId;
    }
}
