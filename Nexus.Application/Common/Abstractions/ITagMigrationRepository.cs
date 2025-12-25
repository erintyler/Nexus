using Nexus.Application.Features.ImagePosts.Common.Models;
using Nexus.Domain.Entities;
using Nexus.Domain.Events.Tags;
using Nexus.Domain.Primitives;

namespace Nexus.Application.Common.Abstractions;

/// <summary>
/// Repository abstraction for TagMigration operations.
/// Provides data access methods for tag migration workflows.
/// </summary>
public interface ITagMigrationRepository
{
    /// <summary>
    /// Check if a migration already exists for the given source tag.
    /// </summary>
    Task<TagMigration?> GetMigrationBySourceAsync(TagData source, CancellationToken ct);

    /// <summary>
    /// Get all migrations where the target tag matches the specified tag.
    /// These are "upstream" migrations that will need to be updated.
    /// </summary>
    Task<List<TagMigration>> GetUpstreamMigrationsAsync(TagData targetTag, CancellationToken ct);

    /// <summary>
    /// Get all posts that contain the specified tag.
    /// Returns as an async enumerable for efficient batch processing.
    /// </summary>
    IAsyncEnumerable<ImagePostReadModel> GetPostsWithTagAsync(TagData tag, CancellationToken ct);

    /// <summary>
    /// Store a new tag migration document.
    /// </summary>
    Task CreateMigrationAsync(TagMigration migration, CancellationToken ct);

    /// <summary>
    /// Update multiple migrations in a single transaction.
    /// Deletes the old migrations and creates new ones.
    /// </summary>
    Task UpdateMigrationsAsync(
        List<TagMigration> toDelete,
        List<TagMigration> toCreate,
        CancellationToken ct);

    /// <summary>
    /// Append TagMigrated events to multiple ImagePost aggregates in batches.
    /// </summary>
    Task AppendMigrationEventsBatchAsync(
        List<Guid> postIds,
        TagMigratedDomainEvent domainEvent,
        CancellationToken ct);
}

