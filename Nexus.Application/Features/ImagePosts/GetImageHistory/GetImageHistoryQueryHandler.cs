using Marten;
using Marten.Pagination;
using Nexus.Application.Common.Pagination;
using Nexus.Application.Extensions;
using Nexus.Domain.Common;
using Nexus.Domain.Errors;

namespace Nexus.Application.Features.ImagePosts.GetImageHistory;

public static class GetImageHistoryQueryHandler
{
    public static async Task<Result<PagedResult<string>>> Handle(
        GetImageHistoryQuery request, 
        IQuerySession querySession,
        CancellationToken cancellationToken)
    {
        var query = querySession.Events
            .QueryAllRawEvents()
            .Where(e => e.StreamId == request.Id);
        
        if (request.DateFrom.HasValue) 
        {
            query = query.Where(e => e.Timestamp >= request.DateFrom.Value);
        }
        
        if (request.DateTo.HasValue) 
        {
            query = query.Where(e => e.Timestamp <= request.DateTo.Value);
        }
        
        var totalCount = await query.CountAsync(cancellationToken);
        
        if (totalCount == 0)
        {
            return ImagePostErrors.NotFound;
        }

        var events = await query
            .OrderByDescending(e => e.Timestamp)
            .ToPagedResultAsync(request, cancellationToken);

        foreach (var e in events.Items)
        {
            
        }
    }
    
    
}