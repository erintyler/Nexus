using Nexus.Application.Common.Services;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Primitives;
using Wolverine.Marten;

namespace Nexus.Application.Features.ImagePosts.CreateImagePost;

public class CreateImagePostCommandHandler
{
    public static async Task<(Result<CreateImagePostResponse>, IStartStream?)> Handle(
        CreateImagePostCommand request,
        IUserContextService userContextService,
        ITagMigrationService tagMigrationService,
        CancellationToken ct)
    {
        var userId = userContextService.GetUserId();
        
        var tags = request.Tags
            .Select(t => new TagData(t.Type, t.Value))
            .ToList();
        
        // Resolve any tag migrations before creating the post
        var resolvedTags = await tagMigrationService.ResolveMigrationsAsync(tags, ct);
        
        // Let the aggregate handle validation and event creation
        var createEventResult = ImagePost.Create(userId, request.Title, resolvedTags);
        
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

