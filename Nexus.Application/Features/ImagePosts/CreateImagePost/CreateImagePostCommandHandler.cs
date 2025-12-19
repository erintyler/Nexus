using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Events.ImagePosts;
using Nexus.Domain.ValueObjects;
using Wolverine.Marten;

namespace Nexus.Application.Features.ImagePosts.CreateImagePost;

public static class CreateImagePostCommandHandler
{
    public static (Result, IStartStream?) Handle(CreateImagePostCommand request)
    {
        var tagResults = request.Tags
            .Select(t => Tag.Create(t.TagValue, t.TagType))
            .ToList();
        
        if (tagResults.Any(tr => tr.IsFailure))
        {
            var firstError = tagResults.First(tr => tr.IsFailure).Error;
            return (firstError, null);
        }
        
        var tags = tagResults.Select(tr => tr.Value).ToList();
        
        var createEvent = new ImagePostCreatedDomainEvent(Guid.NewGuid(), request.Title, tags);
        var stream = MartenOps.StartStream<ImagePost>(createEvent);
        
        return (Result.Success(), stream);
    }
}