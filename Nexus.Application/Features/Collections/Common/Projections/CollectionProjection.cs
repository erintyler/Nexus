using Marten;
using Marten.Events.Aggregation;
using Nexus.Application.Features.Collections.Common.Models;
using Nexus.Application.Features.ImagePosts.Common.Models;
using Nexus.Domain.Events.Collections;

namespace Nexus.Application.Features.Collections.Common.Projections;

/// <summary>
/// Marten projection for Collection - builds a read model from domain events.
/// This creates a denormalized view optimized for queries.
/// Aggregates tags from all child image posts.
/// </summary>
public class CollectionProjection : SingleStreamProjection<CollectionReadModel, Guid>
{
    public static void Apply(CollectionReadModel readModel, CollectionCreatedDomainEvent @event)
    {
        readModel.Title = @event.Title;
        readModel.CreatedBy = @event.UserId.ToString();
    }

    public static async Task ApplyAsync(
        IDocumentOperations ops,
        CollectionReadModel readModel, 
        ImagePostAddedToCollectionDomainEvent @event)
    {
        readModel.ImagePostIds.Add(@event.ImagePostId);

        // Query the image post to get its tags and aggregate them
        var imagePost = await ops.LoadAsync<ImagePostReadModel>(@event.ImagePostId);
        if (imagePost != null)
        {
            // Add tags from the image post that aren't already in the collection
            foreach (var tag in imagePost.Tags)
            {
                var tagExists = readModel.AggregatedTags.Any(t =>
                    t.Type == tag.Type &&
                    t.Value == tag.Value);

                if (!tagExists)
                {
                    readModel.AggregatedTags.Add(new TagReadModel(tag.Value, tag.Type));
                }
            }
        }
    }

    public static async Task ApplyAsync(
        IDocumentOperations ops,
        CollectionReadModel readModel, 
        ImagePostRemovedFromCollectionDomainEvent @event)
    {
        readModel.ImagePostIds.Remove(@event.ImagePostId);

        // Rebuild aggregated tags from remaining image posts
        await RebuildAggregatedTagsAsync(ops, readModel);
    }

    private static async Task RebuildAggregatedTagsAsync(IDocumentOperations ops, CollectionReadModel readModel)
    {
        readModel.AggregatedTags.Clear();

        // Load all image posts in the collection
        var imagePosts = await ops.LoadManyAsync<ImagePostReadModel>(readModel.ImagePostIds);

        // Aggregate all unique tags from the image posts
        var allTags = imagePosts
            .SelectMany(ip => ip.Tags)
            .Distinct(new TagReadModelComparer())
            .Select(t => new TagReadModel(t.Value, t.Type))
            .ToList();

        readModel.AggregatedTags.AddRange(allTags);
    }

    private class TagReadModelComparer : IEqualityComparer<TagReadModel>
    {
        public bool Equals(TagReadModel? x, TagReadModel? y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return x.Type == y.Type && x.Value == y.Value;
        }

        public int GetHashCode(TagReadModel obj)
        {
            return HashCode.Combine(obj.Type, obj.Value);
        }
    }
}
