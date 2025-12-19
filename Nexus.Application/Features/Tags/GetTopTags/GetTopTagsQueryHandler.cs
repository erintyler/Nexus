using Marten;
using Nexus.Application.Common.Pagination;
using Nexus.Application.Extensions;
using Nexus.Application.Features.Tags.Common.Projections;
using Nexus.Domain.Common;
using Nexus.Domain.Errors;

namespace Nexus.Application.Features.Tags.GetTopTags;

public static class GetTopTagsQueryHandler
{
    public static async Task<Result<PagedResult<TagCount>>> Handle(GetTopTagsQuery request, IQuerySession session, CancellationToken cancellationToken = default)
    {
        var tagQuery = session.Query<TagCount>()
            .OrderByDescending(x => x.Count);

        var totalCount = await tagQuery.CountAsync(cancellationToken);

        if (totalCount == 0)
        {
            return TagErrors.NoResults;
        }

        return await tagQuery.ToPagedResultAsync(request, cancellationToken);
    }
}