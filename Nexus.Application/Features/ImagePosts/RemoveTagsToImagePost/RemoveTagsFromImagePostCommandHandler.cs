using Nexus.Application.Common.Services;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Primitives;
using Wolverine.Marten;
using Wolverine.Persistence;

namespace Nexus.Application.Features.ImagePosts.RemoveTagsToImagePost;

public class RemoveTagsFromImagePostCommandHandler
{
    public static async Task<(Result, Events)> HandleAsync(
        RemoveTagsFromImagePostCommand request,
        [WriteAggregate(OnMissing = OnMissing.ProblemDetailsWith404)] ImagePost imagePost,
        CancellationToken ct)
    {
        var tags = request.Tags
            .Select(t => new TagData(t.Type, t.Value))
            .ToList();

        var result = imagePost.RemoveTags(tags);

        if (result.IsFailure)
        {
            return (result, []);
        }

        return (Result.Success(), [.. result.Value]);
    }
}

