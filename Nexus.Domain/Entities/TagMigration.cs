using Nexus.Domain.Common;
using Nexus.Domain.Errors;
using Nexus.Domain.Events.Tags;
using Nexus.Domain.Primitives;
using Nexus.Domain.ValueObjects;

namespace Nexus.Domain.Entities;

public class TagMigration
{
    private TagMigration() { }
    
    public Guid Id { get; private set; }
    public Tag SourceTag { get; private set; } = null!;
    public Tag TargetTag { get; private set; } = null!;
    
    public string CreatedBy { get; private set; } = null!;
    
    // Marten metadata
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset LastModified { get; private set; }
    public string LastModifiedBy { get; private set; } = null!;
    
    public static Result<TagMigratedDomainEvent> Create(Guid userId, TagData source, TagData target)
    {
        if (userId == Guid.Empty)
        {
            return TagMigrationErrors.UserIdEmpty;
        }
        
        var sourceTagResult = Tag.Create(source.Type, source.Value);
        if (sourceTagResult.IsFailure)
        {
            return Result.Failure<TagMigratedDomainEvent>(sourceTagResult.Errors);
        }
        
        var targetTagResult = Tag.Create(target.Type, target.Value);
        if (targetTagResult.IsFailure)
        {
            return Result.Failure<TagMigratedDomainEvent>(targetTagResult.Errors);
        }
        
        return new TagMigratedDomainEvent(userId, source, target);
    }
    
    public void Apply(TagMigratedDomainEvent @event)
    {
        SourceTag = new Tag(@event.Source.Type, @event.Source.Value);
        TargetTag = new Tag(@event.Target.Type, @event.Target.Value);
        CreatedBy = @event.UserId.ToString();
    }
}