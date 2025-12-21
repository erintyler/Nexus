using System.Text.Json.Serialization;
using Nexus.Domain.Abstractions;
using Nexus.Domain.Events.Comments;
using Nexus.Domain.Events.ImagePosts;
using Nexus.Domain.Events.Tags;
using Nexus.Domain.ValueObjects;

namespace Nexus.Domain.Entities;

public class ImagePost : ITaggable
{
    [JsonInclude]
    [JsonPropertyName("Comments")]
    private List<Comment> _comments = [];
    
    [JsonInclude]
    [JsonPropertyName("Tags")]
    private HashSet<Tag> _tags = [];
    
    [JsonConstructor]
    private ImagePost() { }
    
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTimeOffset LastModified { get; set; }
    public string LastModifiedBy { get; set; }
    
    [JsonIgnore]
    public IReadOnlyList<Comment> Comments => _comments.AsReadOnly();
    
    [JsonIgnore]
    public IReadOnlySet<Tag> Tags => _tags.AsReadOnly();
    
    public void Apply(ImagePostCreatedDomainEvent @event)
    {
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
        // Use constructor instead of Tag.Create since tag was validated before event was created
        _tags.Add(new Tag(@event.TagValue, @event.TagType));
    }
}