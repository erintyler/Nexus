using Marten;
using Nexus.Application.Common.Abstractions;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;

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

    public async Task<Result<Guid>> CreateAsync(string discordId, string discordUsername, CancellationToken ct)
    {
        // Use the factory method to validate and create the domain event
        var createResult = User.Create(discordId, discordUsername);

        if (createResult.IsFailure)
        {
            return Result.Failure<Guid>(createResult.Errors);
        }

        var userId = Guid.NewGuid();
        session.Events.StartStream<User>(userId, createResult.Value);
        await session.SaveChangesAsync(ct);

        return Result.Success(userId);
    }
}
