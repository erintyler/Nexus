using Marten;
using Nexus.Application.Common.Abstractions;
using Nexus.Application.Features.ImagePosts.Common.Models;
using Nexus.Domain.Entities;
using Nexus.Domain.Events.Tags;
using Nexus.Domain.Primitives;

namespace Nexus.Infrastructure.Repositories;

/// <summary>
/// Marten-based implementation of ITagMigrationRepository.
/// Provides data access for tag migration operations using Marten document store.
/// </summary>
public class TagMigrationRepository(IDocumentSession session) : ITagMigrationRepository
{
    public async Task<TagMigration?> GetMigrationBySourceAsync(TagData source, CancellationToken ct)
    {
        return await session
            .Query<TagMigration>()
            .Where(m => m.SourceTag.Type == source.Type && m.SourceTag.Value == source.Value)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<TagMigration>> GetUpstreamMigrationsAsync(TagData targetTag, CancellationToken ct)
    {
        var results = await session
            .Query<TagMigration>()
            .Where(m => m.TargetTag.Type == targetTag.Type && m.TargetTag.Value == targetTag.Value)
            .ToListAsync(ct);

        return results.ToList();
    }

    public IAsyncEnumerable<ImagePostReadModel> GetPostsWithTagAsync(TagData tag, CancellationToken ct)
    {
        return session
            .Query<ImagePostReadModel>()
            .Where(p => p.Tags.Any(t => t.Type == tag.Type && t.Value == tag.Value))
            .ToAsyncEnumerable(ct);
    }

    public async Task CreateMigrationAsync(TagMigration migration, CancellationToken ct)
    {
        session.Store(migration);
        await session.SaveChangesAsync(ct);
    }

    public async Task UpdateMigrationsAsync(
        List<TagMigration> toDelete,
        List<TagMigration> toCreate,
        CancellationToken ct)
    {
        foreach (var migration in toDelete)
        {
            session.Delete(migration);
        }

        foreach (var migration in toCreate)
        {
            session.Store(migration);
        }

        await session.SaveChangesAsync(ct);
    }

    public async Task AppendMigrationEventsBatchAsync(
        List<Guid> postIds,
        TagMigratedDomainEvent domainEvent,
        CancellationToken ct)
    {
        foreach (var postId in postIds)
        {
            session.Events.Append(postId, domainEvent);
        }

        await session.SaveChangesAsync(ct);
    }
}

