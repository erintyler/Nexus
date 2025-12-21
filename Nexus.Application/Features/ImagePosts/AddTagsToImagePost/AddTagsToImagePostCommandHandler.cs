using Marten;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Errors;
using Nexus.Domain.ValueObjects;
using Wolverine.Http.Marten;
using Wolverine.Marten;

namespace Nexus.Application.Features.ImagePosts.AddTagsToImagePost;

public class AddTagsToImagePostCommandHandler
{
    public static Result Handle(
        AddTagsToImagePostCommand request, 
        IDocumentSession session,
        [Aggregate] ImagePost? imagePost)
    {
        if (imagePost is null)
        {
            return ImagePostErrors.NotFound;
        }
        
        // Convert DTOs to domain value objects - let Tag.Create handle validation
        var tagResults = request.Tags
            .Select(t => Tag.Create(t.Value, t.Type))
            .ToList();
        
        // Extract tags for aggregate
        var tags = tagResults
            .Where(t => t.IsSuccess)
            .Select(t => t.Value)
            .ToList();
        
        // Let the aggregate handle validation and event creation
        var addTagsResult = imagePost.AddTags(tags);
        
        if (addTagsResult.IsFailure)
        {
            return Result.Failure(addTagsResult.Errors);
        }
        
        // Append the events to the stream
        foreach (var @event in addTagsResult.Value)
        {
            session.Events.Append(request.ImagePostId, @event);
        }
        
        return Result.Success();
    }
}

