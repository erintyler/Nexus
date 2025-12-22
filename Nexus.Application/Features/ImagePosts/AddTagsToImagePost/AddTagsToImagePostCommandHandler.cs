using Nexus.Application.Common.Services;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Primitives;
using Wolverine.Marten;
using Wolverine.Persistence;

namespace Nexus.Application.Features.ImagePosts.AddTagsToImagePost;

public class AddTagsToImagePostCommandHandler
{
    public static async Task<(Result, Events)> Handle(
        AddTagsToImagePostCommand request, 
        [WriteAggregate(OnMissing = OnMissing.ProblemDetailsWith404)] ImagePost imagePost,
        ITagMigrationService tagMigrationService,
        CancellationToken ct)
    {
        var tags = request.Tags
            .Select(t => new TagData(t.Type, t.Value))
            .ToList();
        
        // Resolve any tag migrations before adding tags
        var resolvedTags = await tagMigrationService.ResolveMigrationsAsync(tags, ct);
        
        var result = imagePost.AddTags(resolvedTags);
        
        if (result.IsFailure)
        {
            return (result, []);
        }

        return (Result.Success(), [..result.Value]);
    }
}

