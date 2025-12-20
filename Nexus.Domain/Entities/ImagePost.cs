using Nexus.Domain.Abstractions;
using Nexus.Domain.Common;
using Nexus.Domain.Errors;
using Nexus.Domain.Events.Comments;
using Nexus.Domain.Events.ImagePosts;
using Nexus.Domain.Events.Tags;
using Nexus.Domain.ValueObjects;

namespace Nexus.Domain.Entities;

public class ImagePost : ITaggable
{
    private readonly List<Comment> _comments = [];
    private readonly HashSet<Tag> _tags = [];
    
    private ImagePost() { }
    
    public Guid Id { get; private set; }
    public string Title { get; private set; } = null!;
    
    public IReadOnlyList<Comment> Comments => _comments.AsReadOnly();
    public IReadOnlySet<Tag> Tags => _tags.AsReadOnly();
    
    public void Apply(ImagePostCreatedDomainEvent @event)
    {
        Id = @event.Id;
        Title = @event.Title;
        
        foreach (var tag in @event.Tags)
        {
            _tags.Add(tag);
        }
    }
    
    public void Apply(CommentCreatedDomainEvent @event)
    {
        var comment = new Comment(@event.Id, @event.UserId, @event.Content);
        _comments.Add(comment);
    }
    
    public void Apply(CommentUpdatedDomainEvent @event) 
    {
        var comment = _comments.First(c => c.Id == @event.Id);
        comment.Content = @event.Content;
    }
    
    public void Apply(CommentDeletedDomainEvent @event)
    {
        var comment = _comments.First(c => c.Id == @event.Id);
        _comments.Remove(comment);
    }
    
    public void Apply(TagAddedDomainEvent @event)
    {
        var tag = Tag.Create(@event.TagValue, @event.TagType);
        _tags.Add(tag.Value);
    }
}