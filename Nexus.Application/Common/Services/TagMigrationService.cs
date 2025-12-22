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
        var resolved = new List<TagData>();
        
        foreach (var tag in tags)
        {
            var target = await GetTargetTagAsync(tag, ct);
            resolved.Add(target ?? tag);

            if (target is not null)
            {
                logger.LogInformation("Tag resolved: Source: {@SourceTag}, Resolved: {@ResolvedTag}", tag, target);
            }
        }
        
        return resolved;
    }
}

