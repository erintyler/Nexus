using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Events.ImagePosts;
using Nexus.Domain.ValueObjects;
using Wolverine.Marten;

namespace Nexus.Application.Features.ImagePosts.CreateImagePost;

public static class CreateImagePostCommandHandler
{
    public static (Result<Guid>, IStartStream?) Handle(CreateImagePostCommand request)
    {
        var tagResults = request.Tags
            .Select(t => Tag.Create(t.Value, t.Type))
            .ToList();

        var errors = tagResults
            .Where(r => r.IsFailure)
            .SelectMany(r => r.Errors)
            .ToList();
        
        if (errors.Count != 0)
        {
            return (Result.Failure<Guid>(errors), null);
        }
        
        var tags = tagResults.Select(tr => tr.Value).ToList();
        
        var id = Guid.NewGuid();
        var createEvent = new ImagePostCreatedDomainEvent(id, request.Title, tags);
        var stream = MartenOps.StartStream<ImagePost>(createEvent);
        
        return (id, stream);
    }
}