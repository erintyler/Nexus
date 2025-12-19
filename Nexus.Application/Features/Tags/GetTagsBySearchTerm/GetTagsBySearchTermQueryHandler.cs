using Marten;
using Nexus.Application.Common.Pagination;
using Nexus.Application.Extensions;
using Nexus.Application.Features.Tags.Common.Projections;
using Nexus.Domain.Common;
using Nexus.Domain.Errors;

namespace Nexus.Application.Features.Tags.GetTagsBySearchTerm;

public static class GetTagsBySearchTermQueryHandler
{
    public static async Task<Result<PagedResult<TagCount>>> Handle(GetTagsBySearchTermQuery request, IQuerySession session, CancellationToken cancellationToken = default)
    {
        var searchQuery =  session.Query<TagCount>()
            .Where(x => x.TagValue.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase));
        
        var totalCount = await searchQuery.CountAsync(cancellationToken);
        
        if (totalCount == 0)
        {
            return TagErrors.NoResults;
        }

        return await searchQuery
            .OrderByDescending(x => x.Count)
            .ToPagedResultAsync(request, cancellationToken);
    }
}