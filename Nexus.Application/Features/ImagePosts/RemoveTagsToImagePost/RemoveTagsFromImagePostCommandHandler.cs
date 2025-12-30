using Nexus.Application.Common.Services;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Errors;
using Nexus.Domain.Primitives;
using Wolverine.Marten;
using Wolverine.Persistence;

namespace Nexus.Application.Features.ImagePosts.RemoveTagsToImagePost;

public class RemoveTagsFromImagePostCommandHandler
{
    public static async Task<(Result, Events)> HandleAsync(
        RemoveTagsFromImagePostCommand request,
        [WriteAggregate(Required = false)] ImagePost? imagePost,
        CancellationToken ct)
    {
        if (imagePost is null)
        {
            return (ImagePostErrors.NotFound, []);
        }

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

