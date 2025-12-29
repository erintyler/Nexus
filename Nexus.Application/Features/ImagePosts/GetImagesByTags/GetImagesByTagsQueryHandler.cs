using Marten;
using Nexus.Application.Common.Models;
using Nexus.Application.Common.Pagination;
using Nexus.Application.Common.Services;
using Nexus.Application.Extensions;
using Nexus.Application.Features.ImagePosts.Common.Models;
using Nexus.Application.Features.ImagePosts.GetImageById;
using Nexus.Domain.Common;
using Nexus.Domain.Enums;
using Nexus.Domain.Errors;

namespace Nexus.Application.Features.ImagePosts.GetImagesByTags;

public static class GetImagesByTagsQueryHandler
{
    public static async Task<Result<PagedResult<ImagePostDto>>> HandleAsync(
        GetImagesByTagsQuery request,
        IQuerySession session,
        IImageService imageService,
        CancellationToken cancellationToken = default)
    {
        if (request.Tags == null || request.Tags.Count == 0)
        {
            return ImagePostErrors.NotFound;
        }

        // Build query to find images that have all the requested tags
        var query = session.Query<ImagePostReadModel>()
            .Where(x => x.Status == UploadStatus.Completed);

        // Filter by tags - image must contain all requested tags
        foreach (var requestedTag in request.Tags)
        {
            var tagType = requestedTag.Type;
            var tagValue = requestedTag.Value;
            query = query.Where(x => x.Tags.Any(t => t.Type == tagType && t.Value == tagValue));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        if (totalCount == 0)
        {
            return ImagePostErrors.NotFound;
        }

        var results = await query
            .OrderByDescending(x => x.CreatedAt)
            .ToPagedResultAsync(request, totalCount, cancellationToken);

        // Map to DTOs
        var dtos = results.Items.Select(imagePost =>
        {
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
        }).ToList();

        return PagedResult<ImagePostDto>.Create(dtos, results.TotalCount, request);
    }
}