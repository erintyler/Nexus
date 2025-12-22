using Marten;
using Nexus.Application.Common.Models;
using Nexus.Application.Features.ImagePosts.Common.Models;
using Nexus.Domain.Common;
using Nexus.Domain.Errors;
using Wolverine.Marten;
using Wolverine.Persistence;

namespace Nexus.Application.Features.ImagePosts.GetImageById;

public static class GetImagePostQueryHandler
{
    public static async Task<Result<ImagePostDto>> Handle(GetImagePostQuery request, IQuerySession session, [ReadAggregate] ImagePostReadModel? imagePost)
    {
        if (imagePost is null)
        {
            return ImagePostErrors.NotFound;
        }

        var tags = imagePost.Tags
            .Select(t => new TagDto(t.Type, t.Value))
            .ToList();

        return new ImagePostDto(
            imagePost.Title,
            tags,
            "n/a",
            imagePost.CreatedAt,
            imagePost.LastModified,
            imagePost.CreatedBy,
            imagePost.LastModifiedBy);
    }
}

