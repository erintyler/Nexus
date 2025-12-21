using Nexus.Domain.Enums;

namespace Nexus.Application.Features.ImagePosts.Common.Models;

/// <summary>
/// Read model for ImagePost - optimized for queries and projections.
/// This is a denormalized view that Marten uses for efficient querying.
/// </summary>
public class ImagePostReadModel
{
    // Parameterless constructor for Marten
    public ImagePostReadModel()
    {
        Title = string.Empty;
        CreatedBy = string.Empty;
        LastModifiedBy = string.Empty;
        Tags = [];
        Comments = [];
    }
    
    public Guid Id { get; set; }
    public string Title { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTimeOffset LastModified { get; set; }
    public string LastModifiedBy { get; set; }
    
    public List<TagReadModel> Tags { get; set; }
    public List<CommentReadModel> Comments { get; set; }
}

/// <summary>
/// Read model for Tag within an ImagePost projection
/// </summary>
public class TagReadModel
{
    public TagReadModel()
    {
        Value = string.Empty;
    }
    
    public TagReadModel(string value, TagType type)
    {
        Value = value;
        Type = type;
    }
    
    public string Value { get; set; }
    public TagType Type { get; set; }
}

/// <summary>
/// Read model for Comment within an ImagePost projection
/// </summary>
public class CommentReadModel
{
    public CommentReadModel()
    {
        Content = string.Empty;
    }
    
    public CommentReadModel(Guid id, Guid userId, string content)
    {
        Id = id;
        UserId = userId;
        Content = content;
    }
    
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; }
}

