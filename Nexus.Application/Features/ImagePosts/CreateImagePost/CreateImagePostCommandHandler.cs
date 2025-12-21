using Nexus.Application.Common.Services;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.ValueObjects;
using Wolverine.Marten;

namespace Nexus.Application.Features.ImagePosts.CreateImagePost;

public static class CreateImagePostCommandHandler
{
    public static (Result<CreateImagePostResponse>, IStartStream?) Handle(
        CreateImagePostCommand request,
        IUserContextService userContextService)
    {
        var userId = userContextService.GetUserId();
        
        // Convert DTOs to domain value objects - let Tag.Create handle validation
        var tagResults = request.Tags
            .Select(t => Tag.Create(t.Value, t.Type))
            .ToList();
        
        // Extract tags for aggregate (aggregate will validate them again, but that's OK for double validation)
        var tags = tagResults
            .Where(t => t.IsSuccess)
            .Select(t => t.Value)
            .ToList();
        
        // Let the aggregate handle validation and event creation
        var createEventResult = ImagePost.Create(userId, request.Title, tags);
        
        if (createEventResult.IsFailure)
        {
            return (Result.Failure<CreateImagePostResponse>(createEventResult.Errors), null);
        }
        
        // Start the event stream with the created event
        var stream = MartenOps.StartStream<ImagePost>(createEventResult.Value);
        var response = new CreateImagePostResponse(stream.StreamId, request.Title, DateTime.UtcNow);
        
        return (response, stream);
    }
}

