using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Events.ImagePosts;
using Nexus.Domain.Extensions;
using Nexus.Domain.ValueObjects;
using Wolverine.Marten;

namespace Nexus.Application.Features.ImagePosts.CreateImagePost;

public static class CreateImagePostCommandHandler
{
    public static (Result<CreateImagePostResponse>, IStartStream?) Handle(CreateImagePostCommand request)
    {
        var tagResults = request.Tags
            .Select(t => Tag.Create(t.Value, t.Type))
            .ToList();

        var errors = tagResults
            .WithIndexedErrors(nameof(request.Tags))
            .ToList();
        
        if (errors.Count != 0)
        {
            return (Result.Failure<CreateImagePostResponse>(errors), null);
        }
        
        var tags = tagResults.Select(tr => tr.Value).ToList();
        
        var createEvent = new ImagePostCreatedDomainEvent(request.Title, tags);
        var stream = MartenOps.StartStream<ImagePost>(createEvent);
        
        var response = new CreateImagePostResponse(stream.StreamId, request.Title, DateTime.UtcNow);
        
        return (response, stream);
    }
}