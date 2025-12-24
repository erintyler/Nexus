using Marten;
using Microsoft.Extensions.Logging;
using Nexus.Domain.Entities;
using Nexus.Domain.Primitives;

namespace Nexus.Application.Common.Services;

/// <summary>
/// Service implementation for resolving tag migrations.
/// Queries the TagMigration documents to determine if a tag should be replaced.
/// </summary>
public class TagMigrationService(IDocumentSession session, ILogger<TagMigrationService> logger) : ITagMigrationService
{
    public async Task<TagData?> GetTargetTagAsync(TagData sourceTag, CancellationToken ct = default)
    {
        var migration = await session
            .Query<TagMigration>()
            .Where(m => m.SourceTag.Type == sourceTag.Type && m.SourceTag.Value == sourceTag.Value)
            .FirstOrDefaultAsync(ct);

        return migration is not null 
            ? new TagData(migration.TargetTag.Type, migration.TargetTag.Value) 
            : null;
    }

    public async Task<IReadOnlyList<TagData>> ResolveMigrationsAsync(
        IReadOnlyList<TagData> tags, 
        CancellationToken ct = default)
    {
        if (tags.Count == 0)
            return tags;
        
        var tagTypes = tags.Select(t => t.Type).ToArray();
        var tagValues = tags.Select(t => t.Value).ToArray();
        
        var migrations = await session.Query<TagMigration>()
            .Where(m => m.SourceTag.Type.In(tagTypes) && m.SourceTag.Value.In(tagValues))
            .ToListAsync(ct);

        // Create lookup dictionary for O(1) access
        var migrationLookup = migrations.ToDictionary(
            m => new TagData(m.SourceTag.Type, m.SourceTag.Value),
            m => new TagData(m.TargetTag.Type, m.TargetTag.Value));

        var resolved = new List<TagData>(tags.Count);

        foreach (var tag in tags)
        {
            if (migrationLookup.TryGetValue(tag, out var target))
            {
                resolved.Add(target);
                logger.LogInformation("Tag resolved: Source: {@SourceTag}, Resolved: {@ResolvedTag}", tag, target);
            }
            else
            {
                resolved.Add(tag);
            }
        }

        return resolved;
    }
}

