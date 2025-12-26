using Nexus.Application.Features.ImagePosts.Common.Models;

namespace Nexus.Application.Features.Collections.Common.Models;

/// <summary>
/// Read model for Collection - optimized for queries and projections.
/// This is a denormalized view that Marten uses for efficient querying.
/// Aggregates tags from all child image posts.
/// </summary>
public class CollectionReadModel
{
    // Parameterless constructor for Marten
    public CollectionReadModel()
    {
        Title = string.Empty;
        CreatedBy = string.Empty;
        LastModifiedBy = string.Empty;
        ImagePostIds = [];
        AggregatedTags = [];
    }

    public Guid Id { get; set; }
    public string Title { get; set; }
    public string CreatedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastModified { get; set; }
    public string LastModifiedBy { get; set; }

    public List<Guid> ImagePostIds { get; set; }
    public List<TagReadModel> AggregatedTags { get; set; }
}
