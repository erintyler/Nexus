using Nexus.Application.Common.Services;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Primitives;
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
        
        var tags = request.Tags
            .Select(t => new TagData(t.Type, t.Value))
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

