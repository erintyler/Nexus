using Marten;
using Nexus.Application.Common.Models;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Errors;
using Wolverine.Marten;
using Wolverine.Persistence;

namespace Nexus.Application.Features.ImagePosts.GetImageById;

public static class GetImagePostQueryHandler
{
    public static async Task<Result<ImagePostDto>> Handle(GetImagePostQuery request, IQuerySession session, [ReadAggregate] ImagePost? imagePost)
    {
        if (imagePost is null)
        {
            return ImagePostErrors.NotFound;
        }

        var tags = imagePost.Tags
            .Select(t => new TagDto(t.Value, t.Type))
            .ToList();

        var events = await session.Events.FetchStreamAsync(imagePost.Id, version: 1);
        var firstEvent = events.FirstOrDefault();
        var createdBy = firstEvent?.UserName ?? "Unknown";

        return new ImagePostDto(
            imagePost.Title,
            tags,
            "n/a",
            imagePost.CreatedAt,
            imagePost.LastModified,
            createdBy,
            imagePost.LastModifiedBy);
    }
}