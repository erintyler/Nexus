using System.Text.Json.Serialization;
using Nexus.Domain.Common;
using Nexus.Domain.Errors;
using Nexus.Domain.Events.Tags;
using Nexus.Domain.Primitives;
using Nexus.Domain.ValueObjects;

namespace Nexus.Domain.Entities;

/// <summary>
/// TagMigration document - represents a mapping from a source tag to a target tag.
/// This is stored as a simple document (not event-sourced) for fast lookups.
/// </summary>
public sealed class TagMigration : BaseEntity
{
    [JsonConstructor]
    internal TagMigration() { } // For Marten
    
    public Guid Id { get; init; }
    public TagData SourceTag { get; init; } = null!;
    public TagData TargetTag { get; init; } = null!;
    
    public string CreatedBy { get; init; } = null!;
    
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset LastModified { get; private set; }
    public string LastModifiedBy { get; private set; } = null!;
    
    /// <summary>
    /// Factory method to create a new TagMigration document with validation.
    /// </summary>
    public static Result<TagMigration> Create(Guid userId, TagData source, TagData target)
    {
        if (userId == Guid.Empty)
        {
            return TagMigrationErrors.UserIdEmpty;
        }
        
        var sourceTagResult = Tag.Create(source.Type, source.Value);
        if (sourceTagResult.IsFailure)
        {
            return Result.Failure<TagMigration>(sourceTagResult.Errors);
        }
        
        var targetTagResult = Tag.Create(target.Type, target.Value);
        if (targetTagResult.IsFailure)
        {
            return Result.Failure<TagMigration>(targetTagResult.Errors);
        }
        
        return new TagMigration
        {
            Id = Guid.NewGuid(),
            SourceTag = source,
            TargetTag = target,
            CreatedBy = userId.ToString()
        };
    }
    
    /// <summary>
    /// Factory method to create the TagMigratedDomainEvent for bulk migration.
    /// This event will be applied to ImagePost aggregates.
    /// </summary>
    public TagMigratedDomainEvent CreateMigrationEvent(Guid userId)
    {
        return new TagMigratedDomainEvent(
            userId,
            new TagData(SourceTag.Type, SourceTag.Value),
            new TagData(TargetTag.Type, TargetTag.Value)
        );
    }
}