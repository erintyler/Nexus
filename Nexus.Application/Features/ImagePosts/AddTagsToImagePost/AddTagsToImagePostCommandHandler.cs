using Marten;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Errors;
using Nexus.Domain.Primitives;
using Nexus.Domain.ValueObjects;
using Wolverine.Http.Marten;
using Wolverine.Marten;
using Wolverine.Persistence;

namespace Nexus.Application.Features.ImagePosts.AddTagsToImagePost;

public class AddTagsToImagePostCommandHandler
{
    public static (Result, Events) Handle(AddTagsToImagePostCommand request, [WriteAggregate(OnMissing = OnMissing.ProblemDetailsWith404)] ImagePost imagePost)
    {
        var tags = request.Tags
            .Select(t => new TagData(t.Type, t.Value))
            .ToList();
        
        var result = imagePost.AddTags(tags);
        
        if (result.IsFailure)
        {
            return (result, []);
        }

        return (Result.Success(), [..result.Value]);
    }
}

