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
    public string CreatedBy { get; set; }
    public UploadStatus Status { get; set; }
    public string? Url { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastModified { get; set; }
    public string LastModifiedBy { get; set; }

    public List<TagReadModel> Tags { get; set; }
    public List<CommentReadModel> Comments { get; set; }
}

