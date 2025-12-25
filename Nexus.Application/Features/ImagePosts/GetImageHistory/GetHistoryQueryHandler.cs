using JasperFx.Events;
using Marten;
using Marten.Events;
using Microsoft.Extensions.Logging;
using Nexus.Application.Common.Models;
using Nexus.Application.Common.Pagination;
using Nexus.Application.Extensions;
using Nexus.Domain.Common;
using Nexus.Domain.Errors;
using Nexus.Domain.Events;

namespace Nexus.Application.Features.ImagePosts.GetImageHistory;

public class GetHistoryQueryHandler
{
    public static async Task<Result<PagedResult<HistoryDto>>> HandleAsync(
        GetHistoryQuery request,
        IQuerySession querySession,
        ILogger<GetHistoryQueryHandler> logger,
        CancellationToken cancellationToken)
    {
        var events = await querySession.Events.FetchStreamAsync(
            request.Id,
            timestamp: request.DateTo,
            token: cancellationToken);

        events = events
            .Where(e => request.DateFrom == null || e.Timestamp >= request.DateFrom)
            .ToList();

        var nexusEvents = events
            .OfType<IEvent<INexusEvent>>()
            .ToList();

        var count = nexusEvents.Count;
        if (count == 0)
        {
            return ImagePostErrors.NotFound;
        }

        var validPaginationRequest = request.WithValidPageNumber(count);

        return nexusEvents
            .Select(e => e.ToHistoryDto())
            .OrderByDescending(e => e.Timestamp)
            .ToPagedResult(validPaginationRequest, count);
    }
}