using Nexus.Application.Common.Models;
using Nexus.Application.Common.Services;
using Nexus.Application.Features.ImagePosts.Common.Models;
using Nexus.Domain.Common;
using Nexus.Domain.Enums;
using Nexus.Domain.Errors;
using Wolverine.Marten;

namespace Nexus.Application.Features.ImagePosts.GetImageById;

public static class GetImagePostQueryHandler
{
    public static async Task<Result<ImagePostDto>> HandleAsync(
        GetImagePostQuery request,
        IImageService imageService,
        [ReadAggregate] ImagePostReadModel? imagePost)
    {
        if (imagePost?.Status is not UploadStatus.Completed)
        {
            return ImagePostErrors.NotFound;
        }

        var tags = imagePost.Tags
            .Select(t => new TagDto(t.Type, t.Value))
            .ToList();

        return new ImagePostDto(
            imagePost.Title,
            tags,
            imageService.GetProcessedImageUrl(imagePost.Id),
            imagePost.CreatedAt,
            imagePost.LastModified,
            imagePost.CreatedBy,
            imagePost.LastModifiedBy);
    }
}

