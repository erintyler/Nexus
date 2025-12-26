namespace Nexus.Application.Features.ImagePosts.Common.Models;

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

