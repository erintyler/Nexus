using Marten;
using Nexus.Application.Common.Pagination;
using Nexus.Application.Extensions;
using Nexus.Application.Features.Tags.Common.Projections;
using Nexus.Domain.Common;
using Nexus.Domain.Errors;

namespace Nexus.Application.Features.Tags.GetTags;

public static class GetTagsQueryHandler
{
    public static async Task<Result<PagedResult<TagCount>>> Handle(GetTagsQuery request, IQuerySession session, CancellationToken cancellationToken = default)
    {
        IQueryable<TagCount> query = session.Query<TagCount>();
        
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.Trim();
            query = query.Where(t => t.Id.Contains(searchTerm));
        }
        
        var totalCount = await query.CountAsync(cancellationToken);
        
        if (totalCount == 0)
        {
            return TagErrors.NoResults;
        }

        return await query
            .OrderByDescending(x => x.Count)
            .ToPagedResultAsync(request, cancellationToken);
    }
}