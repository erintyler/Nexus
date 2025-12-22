using Nexus.Domain.Primitives;

namespace Nexus.Application.Common.Services;

/// <summary>
/// Service for resolving tag migrations.
/// When tags are being added, this service checks if there's a migration rule
/// and automatically replaces the source tag with the target tag.
/// </summary>
public interface ITagMigrationService
{
    /// <summary>
    /// Gets the target tag if a migration exists for the given source tag.
    /// </summary>
    /// <param name="sourceTag">The tag to check for migration</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The target tag if migration exists, null otherwise</returns>
    Task<TagData?> GetTargetTagAsync(TagData sourceTag, CancellationToken ct = default);
    
    /// <summary>
    /// Resolves migrations for a list of tags.
    /// For each tag, if a migration exists, replaces it with the target tag.
    /// </summary>
    /// <param name="tags">The tags to resolve</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of tags with migrations resolved</returns>
    Task<IReadOnlyList<TagData>> ResolveMigrationsAsync(IReadOnlyList<TagData> tags, CancellationToken ct = default);
}

